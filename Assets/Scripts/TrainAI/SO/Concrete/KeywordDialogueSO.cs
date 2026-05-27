using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TrainAI.Core;
using TrainAI.SO.Base;
using UnityEngine;

namespace TrainAI.SO.Concrete
{
    [CreateAssetMenu(fileName = "Dialogue_Keyword", menuName = "TrainAI/Dialogue/Keyword Match")]
    public class KeywordDialogueSO : DialogueStrategySO
    {
        [Serializable]
        public struct IntentRule
        {
            public IntentId intent;
            public string regexPattern;
        }

        public List<IntentRule> rules = new List<IntentRule>
        {
            new IntentRule { intent = IntentId.HOI_LICH, regexPattern = @"(?i)(lịch|hôm nay|nhiệm vụ|làm gì|thời khóa biểu)" },
            new IntentRule { intent = IntentId.HOI_GIO_AN, regexPattern = @"(?i)(ăn|cơm|đói|mấy giờ ăn|nhà ăn)" },
            new IntentRule { intent = IntentId.HOI_VI_TRI, regexPattern = @"(?i)(ở đâu|đường nào|chỗ nào|hướng nào)" },
            new IntentRule { intent = IntentId.HOI_KIEN_THUC, regexPattern = @"(?i)(học|bài|kiến thức|thi|giảng|giáo trình)" },
            new IntentRule { intent = IntentId.BAO_CAO, regexPattern = @"(?i)(xong|hoàn thành|báo cáo|rồi)" },
            new IntentRule { intent = IntentId.XIN_PHEP, regexPattern = @"(?i)(nghỉ|xin phép|vắng|ốm|mệt)" },
            new IntentRule { intent = IntentId.TAM_BIET, regexPattern = @"(?i)(chào|tạm biệt|bye)" }
        };

        public IntentId fallback = IntentId.OUT_OF_SCOPE;

        public override UniTask<string> Reply(string input, NpcContext ctx)
        {
            if (ctx == null || ctx.responseFiller == null)
                return UniTask.FromResult("[Lỗi: Không tìm thấy bộ trả lời]");

            string cleanInput = input.Trim();
            
            // Special case for "đi đâu"
            if (Regex.IsMatch(cleanInput, @"(?i)đi đâu"))
            {
                return UniTask.FromResult("Tôi đi loanh quanh doanh trại thôi. Đồng chí có việc gì không?");
            }

            IntentId matchedIntent = fallback;

            foreach (var rule in rules)
            {
                if (Regex.IsMatch(cleanInput, rule.regexPattern))
                {
                    matchedIntent = rule.intent;
                    break;
                }
            }

            var filled = ctx.responseFiller.Fill(matchedIntent, ctx.fillTemplate);
            return UniTask.FromResult(filled);
        }
    }
}
