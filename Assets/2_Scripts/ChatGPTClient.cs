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
    public float temperature = 0.7f;
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

    private void Awake() { apiKey = LoadFromResources(); }

    private string LoadFromResources()
    {
        try
        {
            TextAsset configFile = Resources.Load<TextAsset>("config");
            if (configFile != null)
            {
                string[] lines = configFile.text.Split('\n');
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                        return line.Substring("OPENAI_API_KEY=".Length).Trim();
                }
            }
        }
        catch (Exception e) { Debug.LogWarning($"Resources 설정 파일 로드 실패: {e.Message}"); }

        return "";
    }

    public void GenerateQuizQuestions(int count = 3, string topic = "일반상식")
    {
        StartCoroutine(RequestQuizQuestions(count, topic));
    }

    private IEnumerator RequestQuizQuestions(int count, string topic)
    {
        string prompt =
            $"아래 형식의 JSON만 출력하세요. 코드펜스, 설명, 주석 금지.\n" +
            $"주제: {topic}\n" +
            "조건:\n" +
            "- 문제 수: " + count + "\n" +
            "- 각 문제 4지선다, 선택지 18자 이내, 문제 80자 이내\n" +
            "- 유형은 상식/넌센스/맞춤법/무작위 중 섞기 가능\n" +
            "- correctAnswerIndex는 0~3 정수\n" +
            "- 간단한 hint 포함\n" +
            "{ \"questions\": [ { \"question\":\"…\", \"answers\":[\"…\",\"…\",\"…\",\"…\"], \"correctAnswerIndex\":0, \"hint\":\"…\" } ] }";

        var request = new ChatGPTRequest
        {
            messages = new[]
            {
                new Message{ role="system", content="You are a JSON API. Output strictly valid JSON with the exact schema requested. No code fences. No prose."},
                new Message{ role="user", content = prompt }
            }
        };

        string jsonRequest = JsonUtility.ToJson(request);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            webRequest.timeout = 30;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string raw = webRequest.downloadHandler.text;
                    Debug.Log("Raw response from ChatGPT:\n" + raw);

                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(raw);
                    if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0].message == null)
                    {
                        Debug.LogError("Invalid response structure from ChatGPT API");
                        yield break;
                    }

                    string content = response.choices[0].message.content;
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Debug.LogError("빈 content");
                        yield break;
                    }

                    // 안전 JSON 추출
                    string jsonContent = ExtractJson(content);
                    Debug.Log("Response (extracted JSON):\n" + jsonContent);

                    QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);
                    if (quizData == null || quizData.questions == null || quizData.questions.Length == 0)
                    {
                        Debug.LogError("JSON 파싱 성공했지만 질문이 없음");
                        yield break;
                    }

                    List<QuestionSO> generatedQuestions = CreateQuestionSOs(quizData.questions);
                    quizGenerateHandler?.Invoke(generatedQuestions);
                }
                catch (Exception e)
                {
                    Debug.LogError($"응답 파싱 오류: {e.Message}");
                    Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"ChatGPT API 요청 실패: {webRequest.error}");
                Debug.LogError($"응답 코드: {webRequest.responseCode}");
                Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
            }
        }
    }

    // 첫 '{' ~ 마지막 '}'만 잘라서 반환. 코드펜스/텍스트 혼입 방어.
    private string ExtractJson(string s)
    {
        s = s.Trim();
        // 코드펜스 제거
        if (s.StartsWith("```"))
        {
            int idx = s.IndexOf('\n');
            if (idx >= 0) s = s.Substring(idx + 1);
            if (s.EndsWith("```")) s = s.Substring(0, s.Length - 3);
        }
        int l = s.IndexOf('{');
        int r = s.LastIndexOf('}');
        if (l >= 0 && r > l) return s.Substring(l, r - l + 1).Trim();
        // 실패 시 원문 반환해 디버깅
        return s;
    }

    private List<QuestionSO> CreateQuestionSOs(QuizQuestion[] quizQuestions)
    {
        var list = new List<QuestionSO>();
        foreach (var q in quizQuestions)
        {
            var so = ScriptableObject.CreateInstance<QuestionSO>();
            so.SetData(q.question, q.answers, q.correctAnswerIndex, q.hint);
            list.Add(so);
        }
        return list;
    }

    public void SetApiKey(string key)
    {
        apiKey = key;
        PlayerPrefs.SetString("OpenAI_API_Key", key);
        PlayerPrefs.Save();
    }

    internal void GenerateQuestions(int questionCount, string topicToUse)
    {
        GenerateQuizQuestions(questionCount, topicToUse);
    }
}
