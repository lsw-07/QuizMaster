using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatGPTRequest
{
    public string model = "gpt-4.1-nano";
    public Message[] messages;
    public float temperature = 0.5f;
    public int max_tokens = 800;
    public ResponseFormat response_format = new ResponseFormat { type = "json_object" };
}
[Serializable] public class ResponseFormat { public string type; }
[Serializable] public class Message { public string role; public string content; }

[Serializable] public class ChatGPTResponse { public Choice[] choices; }
[Serializable] public class Choice { public Message message; }

[Serializable] public class QuizData { public QuizQuestion[] questions; }

[Serializable]
public class QuizQuestion
{
    public string type;                 // 상식 / 넌센스 / 맞춤법
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
    public string hint;
}

public class ChatGPTClient : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private string apiKey;

    public delegate void QuizGenerateHandler(List<QuestionSO> questions);
    public event QuizGenerateHandler quizGenerateHandler;

    private void Awake()
    {
        apiKey = LoadFromResources();
    }

    private string LoadFromResources()
    {
        try
        {
            TextAsset configFile = Resources.Load<TextAsset>("config");
            if (configFile != null)
            {
                string[] lines = configFile.text.Split('\n');
                foreach (string line in lines)
                    if (line.StartsWith("OPENAI_API_KEY="))
                        return line.Substring("OPENAI_API_KEY=".Length).Trim();
            }
        }
        catch (Exception e) { Debug.LogWarning($"Resources 설정 파일 로드 실패: {e.Message}"); }
        return "";
    }

    public void GenerateQuizQuestions(int count = 3, string topic = "상식")
    {
        StartCoroutine(RequestQuizQuestions(count, topic));
    }

    private string NormalizeTopic(string topic)
    {
        topic = (topic ?? "").Trim();
        if (topic.Contains("상식") || topic.Contains("일반")) return "상식";
        if (topic.Contains("넌센스") || topic.Contains("넨센스")) return "넌센스";
        if (topic.Contains("맞춤법")) return "맞춤법";
        if (topic.Contains("무작위")) return "무작위";
        return topic; // 기타 커스텀
    }

    private IEnumerator RequestQuizQuestions(int count, string topic)
    {
        string norm = NormalizeTopic(topic);

        string headerRule = norm == "무작위"
            ? "- 이번 세트에서 임의로 한 가지 유형(type)을 선택하고 모든 문제의 type을 그 한 가지로 동일하게 하세요."
            : $"- 모든 문제의 type은 반드시 \"{norm}\"로 동일하게 하세요. 다른 유형 금지.";

        string prompt =
            "아래 스키마의 JSON만 출력. 코드펜스/설명/주석 금지.\n" +
            headerRule + "\n" +
            $"- 문제 수: {count}\n" +
            "- 각 문제는 4지선다. 문제 120자 이내, 선택지 25자 이내\n" +
            "- correctAnswerIndex는 0~3 정수\n" +
            "- 간단한 hint 포함\n" +
            "스키마 예시: {\n" +
            "  \"questions\": [\n" +
            "    { \"type\":\"상식\", \"question\":\"문제\",\n" +
            "      \"answers\":[\"보기1\",\"보기2\",\"보기3\",\"보기4\"],\n" +
            "      \"correctAnswerIndex\":0,\n" +
            "      \"hint\":\"힌트\" }\n" +
            "  ]\n" +
            "}";

        var req = new ChatGPTRequest
        {
            messages = new[]
            {
                new Message{ role="system",
                    content="You are a strict JSON API. Output ONLY valid JSON that matches the schema. No code fences. No prose." },
                new Message{ role="user", content = prompt }
            }
        };

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            string jsonRequest = JsonUtility.ToJson(req);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            webRequest.timeout = 30;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ChatGPT 요청 실패: {webRequest.error}");
                Debug.LogError($"응답 코드: {webRequest.responseCode}");
                Debug.LogError($"본문: {webRequest.downloadHandler.text}");
                quizGenerateHandler?.Invoke(new List<QuestionSO>()); // 실패 알림
                yield break;
            }

            try
            {
                string raw = webRequest.downloadHandler.text;
                ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(raw);
                if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0].message == null)
                {
                    Debug.LogError("Invalid response structure");
                    quizGenerateHandler?.Invoke(new List<QuestionSO>());
                    yield break;
                }

                string content = response.choices[0].message.content?.Trim();
                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogError("빈 content");
                    quizGenerateHandler?.Invoke(new List<QuestionSO>());
                    yield break;
                }

                // 코드펜스 제거 및 JSON 추출
                string jsonContent = ExtractJson(content);
                QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);

                if (quizData == null || quizData.questions == null || quizData.questions.Length == 0)
                {
                    Debug.LogError("파싱 성공했지만 질문 없음");
                    quizGenerateHandler?.Invoke(new List<QuestionSO>());
                    yield break;
                }

                // 1) 유형 필터: 요청한 유형과 정확히 일치하는 문제만
                string wanted = NormalizeTopic(norm);
                List<QuizQuestion> filtered = new List<QuizQuestion>();
                foreach (var q in quizData.questions)
                {
                    string qt = NormalizeTopic(q.type);
                    if (norm == "무작위" || qt == wanted) filtered.Add(q);
                }

                // 2) 필터 결과가 비면 실패로 간주
                if (filtered.Count == 0)
                {
                    Debug.LogWarning("유형 불일치로 모두 제거됨");
                    quizGenerateHandler?.Invoke(new List<QuestionSO>());
                    yield break;
                }

                // 3) ScriptableObject 변환
                List<QuestionSO> list = new List<QuestionSO>();
                foreach (var q in filtered)
                {
                    var so = ScriptableObject.CreateInstance<QuestionSO>();
                    so.SetData(q.question, q.answers, q.correctAnswerIndex, q.hint);
                    list.Add(so);
                }

                quizGenerateHandler?.Invoke(list);
            }
            catch (Exception e)
            {
                Debug.LogError($"응답 파싱 오류: {e.Message}");
                Debug.LogError($"본문: {webRequest.downloadHandler.text}");
                quizGenerateHandler?.Invoke(new List<QuestionSO>());
            }
        }
    }

    // 코드펜스/텍스트 섞임 방어
    private string ExtractJson(string s)
    {
        s = s.Trim();
        if (s.StartsWith("```"))
        {
            int idx = s.IndexOf('\n');
            if (idx >= 0) s = s.Substring(idx + 1);
            if (s.EndsWith("```")) s = s.Substring(0, s.Length - 3);
        }
        int l = s.IndexOf('{');
        int r = s.LastIndexOf('}');
        if (l >= 0 && r > l) return s.Substring(l, r - l + 1).Trim();
        return s;
    }

    // (미사용) 외부에서 직접 호출 시
    internal void GenerateQuestions(int questionCount, string topicToUse)
    {
        GenerateQuizQuestions(questionCount, topicToUse);
    }
}
