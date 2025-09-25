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
    public float temperature = 1.1f;
    public int max_completion_tokens = 4000;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatGPTResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class QuizData
{
    public QuizQuestion[] questions;
}

[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
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
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                    {
                        return line.Substring("OPENAI_API_KEY=".Length).Trim();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Resources 설정 파일 로드 실패: {e.Message}");
        }

        return "";
    }

    public void GenerateQuizQuestions(int count = 3, string topic = "일반상식")
    {
        StartCoroutine(RequestQuizQuestions(count, topic));
    }

    private IEnumerator RequestQuizQuestions(int count, string topic)
    {
        string prompt = $"다음 조건에 맞는 창의적이고 재미있는 객관식 퀴즈 문제를 {count}개 생성해주세요:\n" +
                       $"주제: {topic}\n" +
                       "조건:\n" +
                       "- 각 문제는 4개의 독창적이고 참신한 선택지를 가져야 합니다\n" +
                       "- 문제는 다양한 난이도와 유형으로 구성해주세요 (기초지식, 추론문제, 상식퀴즈, 창의적 사고 등)\n" +
                       "- 선택지는 함정이 있거나 재치있게 구성해주세요\n" +
                       "- 정답은 0~3 사이의 인덱스로 표시해주세요\n" +
                       "_ 글자수 제한: 문제당 120자 이내, 선택지당 25자 이내\n" +
                       "- 문제와 선택지는 흥미롭고 참여하고 싶게 만들어주세요\n" +
                       "- 가능하면 실생활과 연관된 예시나 시나리오를 활용해주세요\n" +
                       "- 응답은 반드시 다음 JSON 형식으로만 제공해주세요:\n" +
                       "{\n" +
                       "  \"questions\": [\n" +
                       "    {\n" +
                       "      \"question\": \"문제 내용\",\n" +
                       "      \"answers\": [\"선택지1\", \"선택지2\", \"선택지3\", \"선택지4\"],\n" +
                       "      \"correctAnswerIndex\": 0\n" +
                       "    }\n" +
                       "  ]\n" +
                       "}";

        Debug.Log("Prompt to ChatGPT:\n" + prompt);

        ChatGPTRequest request = new ChatGPTRequest
        {
            messages = new Message[]
            {
                new Message { role = "user", content = prompt }
            }
        };

        string jsonRequest = JsonUtility.ToJson(request);
        Debug.Log("Request JSON:\n" + jsonRequest);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Raw response from ChatGPT:\n" + webRequest.downloadHandler.text);
                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(webRequest.downloadHandler.text);

                    if (response == null || response.choices == null || response.choices.Length == 0)
                    {
                        Debug.LogError("Invalid response structure from ChatGPT API");
                        yield break;
                    }

                    if (response.choices[0].message == null)
                    {
                        Debug.LogError("Message content is null in ChatGPT response");
                        yield break;
                    }

                    string jsonContent = response.choices[0].message.content;

                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Debug.LogError("Content is empty. Finish reason: " + response.choices[0].message);
                        Debug.LogError("Consider increasing max_completion_tokens");
                        yield break;
                    }

                    Debug.Log("Response from ChatGPT:\n" + jsonContent);
                    // JSON 문자열에서 불필요한 부분 제거
                    jsonContent = jsonContent.Trim();
                    if (jsonContent.StartsWith("```json"))
                    {
                        jsonContent = jsonContent.Substring(7);
                    }
                    if (jsonContent.EndsWith("```"))
                    {
                        jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
                    }
                    jsonContent = jsonContent.Trim();

                    QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);
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

    private List<QuestionSO> CreateQuestionSOs(QuizQuestion[] quizQuestions)
    {
        List<QuestionSO> questionSOs = new List<QuestionSO>();

        foreach (QuizQuestion quizQ in quizQuestions)
        {
            QuestionSO questionSO = ScriptableObject.CreateInstance<QuestionSO>();

            // Reflection을 사용하여 private 필드에 값 설정
            var questionField = typeof(QuestionSO).GetField("question", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var answersField = typeof(QuestionSO).GetField("answers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var correctAnswerIndexField = typeof(QuestionSO).GetField("correctAnswerIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            questionField?.SetValue(questionSO, quizQ.question);
            answersField?.SetValue(questionSO, quizQ.answers);
            correctAnswerIndexField?.SetValue(questionSO, quizQ.correctAnswerIndex);

            questionSOs.Add(questionSO);
        }

        return questionSOs;
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