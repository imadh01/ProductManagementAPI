using System.Text.Json.Serialization;

namespace ProductManagement.API.Models.Responses;

public class ErrorResponse
{
    [JsonPropertyName("errors")]
    public List<ErrorDetail> Errors { get; set; } = new();
}

public class ErrorDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}