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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.Function.Internet
{
    public class NewVersionInformationClass
    {
        /// <summary>
        /// 测试版版本名
        /// </summary>
        public string PreReleaseVersionName { get; set; }
        /// <summary>
        /// 测试版版本号
        /// </summary>
        public int PreReleaseVersionCode { get; set; }
        /// <summary>
        /// 测试版Github下载地址
        /// </summary>
        public string PreReleaseVersionReleaseFromGithubUrl { get; set; }
        /// <summary>
        /// 测试版Gitee下载地址
        /// </summary>
        public string PreReleaseVersionReleaseFromGiteeUrl { get; set; }
        /// <summary>
        /// 正式版版本名
        /// </summary>
        public string LatestVersionName { get; set; }
        /// <summary>
        /// 正式版版本号
        /// </summary>
        public int LatestVersionCode { get; set; }
        /// <summary>
        /// 正式版Github下载地址
        /// </summary>
        public string LatestVersionReleaseFromGithubUrl { get; set; }
        /// <summary>
        /// 正式版Gitee下载地址
        /// </summary>
        public string LatestVersionReleaseFromGiteeUrl { get; set; }
    }
}
