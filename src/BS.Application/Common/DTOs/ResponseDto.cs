using System;
using System.Collections.Generic;
using System.Text;

namespace BS.Application.Common.DTOs
{
    public class ResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
