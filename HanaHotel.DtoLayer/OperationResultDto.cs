using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanaHotel.DtoLayer
{
    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static OperationResultDto Ok(string msg = "") => new() { Success = true, Message = msg };
        public static OperationResultDto Fail(string msg) => new() { Success = false, Message = msg };
    }
}
