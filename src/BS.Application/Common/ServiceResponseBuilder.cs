using BS.Application.Common.DTOs;
using BS.Application.DTOs;

namespace BS.Application.Common
{
    internal static class ServiceResponseBuilder
    {
        public static void ApplyCsvUploadFailure(ResponseDto<List<BookResponseDto>> response, string message, string error)
        {
            response.IsSuccessful = false;
            response.Status = 400;
            response.Result = 0;
            response.Message = message;
            response.Data = null;
            response.Errors.Clear();
            response.Errors.Add(error);
        }

        public static void ApplyDtoFailure<T>(ResponseDto<T> response, int status, string message, string error)
        {
            response.IsSuccessful = false;
            response.Status = status;
            response.Result = 0;
            response.Message = message;
            response.Data = default;
            response.Errors.Clear();
            response.Errors.Add(error);
        }

        public static void ApplyBookFailure(ResponseDto<BookResponseDto> response, int status, string message, string error)
        {
            ApplyDtoFailure(response, status, message, error);
        }

        public static void ApplyUnexpectedError(BaseResponseDto response, Exception ex)
        {
            response.IsSuccessful = false;
            response.Status = 500;
            response.Result = 0;
            response.Message = "Ocurrió un error inesperado.";
            response.Errors.Clear();
            response.Errors.Add(ex.Message);
        }

        public static void ApplyUnexpectedError<T>(ResponseDto<T> response, Exception ex)
        {
            response.IsSuccessful = false;
            response.Status = 500;
            response.Result = 0;
            response.Message = "Ocurrió un error inesperado.";
            response.Data = default;
            response.Errors.Clear();
            response.Errors.Add(ex.Message);
        }

        public static void ApplyUnexpectedError<T>(PagedResponseDto<T> response, Exception ex)
        {
            response.IsSuccessful = false;
            response.Status = 500;
            response.Result = 0;
            response.Message = "Ocurrió un error inesperado.";
            response.Total = 0;
            response.Data ??= new List<T>();
            response.Data.Clear();
            response.Errors.Clear();
            response.Errors.Add(ex.Message);
        }
    }
}
