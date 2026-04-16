using System;
using System.Collections.Generic;
using System.Text;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string ErrorMessage { get; init; }

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static Result<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}
