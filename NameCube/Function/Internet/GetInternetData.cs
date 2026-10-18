// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY
using Serilog;
using System;

namespace NameCube.Function.Internet
{
    internal static class GetInternetData
    {
        public static NewVersionInformationClass NewVersionInformationClass = new();
        public static async void RefreshNewVersionInformationClass(bool throwException=false)
        {
            string jsonstring = "";
            try
            {
                if (GlobalVariablesData.config.AllSettings.DownloadWay == 0)
                {
                    jsonstring = await DownloadHelper.DownloadFileAsyncToString("https://raw.githubusercontent.com/haaa4/NameCubeResources/refs/heads/main/Data");
                }
                else
                {
                    jsonstring = await DownloadHelper.DownloadFileAsyncToString("https://raw.giteeusercontent.com/haaa4/NameCubeResources/raw/main/Data?metadata=eyJyIjoibWFpbiIsImZwIjoiRGF0YSIsInVpZCI6MTUyMDc1MzQsInBpZCI6NTA3NjIwNTAsInN0byI6ImdpdC1zaGFyZGluZy1zdG8tNDJ0LTAxNCIsInJwIjoicmVwb3MvMmQvNGIvMmQ0YjJhODQ0YWM5NGMxOTg3N2RkNmJiMmIxNzUwNTQ2M2FjNTZjMmQ5MmU1YmE5YzlmN2I0MjliMzQyMzQ1MC5naXQiLCJpc3AiOnRydWUsImV4cGlyZV9hdCI6MTc5MTcwNTYwMH0&signature=KU5L3IJiYGbZfHgjRihmzD7r1g686mOvFBAIWT0xFuA");
                }
            }
            catch(Exception ex)
            {
                if (throwException)
                    throw new Exception("Failed to download data from the internet.", ex);
                else
                    Log.Warning("Failed to download data from the internet. Exception: {Exception}", ex);
            }
            if (jsonstring == "" || jsonstring == null)
            {
                if(throwException)
                    throw new Exception("Failed to download data from the internet.");
                else 
                    Log.Warning("Failed to download data from the internet.");
            }
            NewVersionInformationClass = System.Text.Json.JsonSerializer.Deserialize<NewVersionInformationClass>(jsonstring);
        }
    }
}
