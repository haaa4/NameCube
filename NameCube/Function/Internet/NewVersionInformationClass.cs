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
using System.Windows.Documents;

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

        // ================= 32 位（x86）安装包地址 =================
        // 程序现在同时发布 x64 / x86 两个安装器，老字段保持不变（老版本程序还要读），
        // 这里追加 X86 字段；老服务器数据里没有这些字段时反序列化出来就是 null。

        /// <summary>
        /// 测试版Github下载地址（32 位安装包）
        /// </summary>
        public string PreReleaseVersionReleaseFromGithubUrlX86 { get; set; }
        /// <summary>
        /// 测试版Gitee下载地址（32 位安装包）
        /// </summary>
        public string PreReleaseVersionReleaseFromGiteeUrlX86 { get; set; }
        /// <summary>
        /// 正式版Github下载地址（32 位安装包）
        /// </summary>
        public string LatestVersionReleaseFromGithubUrlX86 { get; set; }
        /// <summary>
        /// 正式版Gitee下载地址（32 位安装包）
        /// </summary>
        public string LatestVersionReleaseFromGiteeUrlX86 { get; set; }

        /// <summary>
        /// 图片Github下载地址列表
        /// </summary>
        public List<string> PictureUrlGithub { get; set; }
        /// <summary>
        /// 图片Gitee下载地址列表
        /// </summary>
        public List<string> PictureUrlGitee { get; set; }

        /// <summary>
        /// 按当前进程位数挑一个下载地址：64 位进程用原字段，32 位进程用 X86 字段。
        /// 注意：32 位进程绝不能回退到 x64 包——那个安装包装不上，只会让用户白下载一次。
        /// </summary>
        /// <exception cref="InvalidOperationException">对应位数的地址缺失</exception>
        public static string SelectReleaseUrl(string x64Url, string x86Url)
        {
            if (Environment.Is64BitProcess)
            {
                if (string.IsNullOrWhiteSpace(x64Url))
                {
                    throw new InvalidOperationException("服务器没有提供 64 位安装包地址。");
                }

                return x64Url;
            }

            if (string.IsNullOrWhiteSpace(x86Url))
            {
                throw new InvalidOperationException("该版本还没有提供 32 位安装包，请等待后续版本。");
            }

            return x86Url;
        }
    }
}
