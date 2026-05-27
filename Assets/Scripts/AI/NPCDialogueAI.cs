using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class NPCDialogueAI : MonoBehaviour
{
    [Header("NPC Personality Setup")]
    public string npcName = "Đại Đội Trưởng";
    
    [TextArea(5, 10)]
    public string systemPrompt = "Bạn là một Đại Đội Trưởng tại khu Giáo dục Quốc phòng (Học kỳ quân đội). Xưng hô là 'Tôi' và gọi người chơi là 'Đồng chí'. Nhiệm vụ của bạn là hướng dẫn, trò chuyện thân thiện nhưng vẫn giữ tác phong quân đội nghiêm trang. Hãy sẵn sàng trả lời chi tiết và sinh động về các kiến thức quân sự (như tháo lắp súng, điều lệnh đội ngũ, nội vụ, kỹ năng sinh tồn) và các sự kiện trong khóa học. Trả lời linh hoạt, tự nhiên như người thật.";

    [Header("UI References")]
    public TMP_InputField chatInputField; // Ô gõ chữ của người chơi

    [Header("Events")]
    public UnityEvent<string> onResponseReceived;
    public UnityEvent<string> onErrorReceived;

    /// <summary>
    /// Hàm này được gọi khi người chơi bấm nút Gửi (Send Button)
    /// </summary>
    public void ChatWithNPC()
    {
        if (chatInputField == null || string.IsNullOrWhiteSpace(chatInputField.text)) 
        {
            Debug.LogWarning("Chưa nhập chữ hoặc chưa gắn InputField!");
            return;
        }

        string playerInput = chatInputField.text;
        Debug.Log($"[{npcName}] Đang suy nghĩ về câu nói: '{playerInput}'...");
        
        LLMNetworkManager.Instance.SendChatRequest(
            systemPrompt, 
            playerInput, 
            onSuccess, 
            onFail
        );
        
        // Xóa trắng ô text sau khi gửi
        chatInputField.text = "";
    }

    private void onSuccess(string aiResponse)
    {
        Debug.Log($"[{npcName} Trả lời]: {aiResponse}");
        // Bắn event để hiển thị lên UI Dialogue Box
        onResponseReceived?.Invoke(aiResponse);
    }

    private void onFail(string errorMsg)
    {
        Debug.LogError($"[API Error] {errorMsg}");
        onErrorReceived?.Invoke(errorMsg);
    }
}
