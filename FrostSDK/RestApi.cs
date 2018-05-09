﻿using FrostSDK.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FrostSDK
{
    public class RestApi
    {
		/// <summary>
		/// HTTP経由でREST APIにアクセスします。
		/// </summary>
		/// <param name="accessToken">ユーザーのアクセストークン</param>
		/// <param name="apiUrl">APIサーバーのURL</param>
		public RestApi(string accessToken, string apiUrl)
		{
			AccessToken = accessToken;
			Requester = new HttpRestApiRequester(apiUrl);
		}

		/// <summary>
		/// 任意のRequesterを使用してREST APIにアクセスします。
		/// </summary>
		/// <param name="accessToken">ユーザーのアクセストークン</param>
		/// <param name="requester">使用するApiRequester</param>
		public RestApi(string accessToken, IRestApiRequester requester)
		{
			AccessToken = accessToken;
			Requester = requester;
		}

		public IRestApiRequester Requester { get; private set; }
		
		public string AccessToken { get; private set; }

		private async Task<RestApiResponse> _Request(HttpMethod method, string endpoint, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null)
		{
			var res = await Requester.RequestAsync(method, endpoint, AccessToken, parameters, headers);

			if (res.StatusCode >= 500)
			{
				throw new Exception("サーバーエラー: " + res.ContentJson);
			}
			else if (res.StatusCode >= 400)
			{
				if (res.StatusCode == 401) throw new Exception("リクエストエラー: 認証に失敗しました");
				if (res.StatusCode == 403) throw new Exception("リクエストエラー: アクセスが許可されていません");
				else throw new Exception($"リクエストエラー({res.StatusCode}): " + res.ContentJson);
			}

			return res;
		}

		/// <summary>
		/// ステータスポストをFrostに投稿します。
		/// </summary>
		/// <param name="text">投稿する本文</param>
		/// <returns></returns>
		public async Task<StatusPost> CreateStatusPost(string text)
		{
			var res = await _Request(HttpMethod.Post, "posts/post_status", new Dictionary<string, string> { ["text"] = text });

			var dic = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(res.ContentJson);

			return StatusPost.FromJson(dic["postStatus"].ToString());
		}
	}
}