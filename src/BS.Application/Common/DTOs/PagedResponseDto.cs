using System;
using System.Collections.Generic;
using System.Text;

namespace BS.Application.Common.DTOs
{
    public class PagedResponseDto<T> : BaseResponseDto
    {
        public long Total { get; set; }
        public List<T>? Data { get; set; }
    }
}
