using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cognitive.CustomVision.Training;
using System.Collections.Generic;
using Microsoft.Rest;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System.Linq;
using Xamarin.Forms;

namespace PartVision.Standard
{

	public static class VisionService
	{
		private static string serviceUri = $"https://southcentralus.api.cognitive.microsoft.com/customvision/v1.2/Training/projects/{PrivateKeys.ProjectId}/images/files?";

		private static HttpClient _visionClient;
		private static HttpClient visionClient
		{
			get
			{
				if (_visionClient != null)
				{
					return _visionClient;
				}
				else
				{
					var client = new HttpClient();
					client.DefaultRequestHeaders.Add("Training-key", PrivateKeys.TrainingKey);

					_visionClient = client;
					return _visionClient;
				}
			}
		}

		/// <summary>
		/// Connects to customvision service api
		/// </summary>
		/// 
		/// upload images for training (including hands)
		/// 
		/// receive notification that image needs classification by someone sighted
		/// 
		/// pull down model

		public static async Task GetUntaggedSets()
		{
			var endpoint = new TrainingApi() { ApiKey = PrivateKeys.TrainingKey };

			var tags = endpoint.GetTags(PrivateKeys.ProjectId);



		}



		public static async Task GetLatestModel()
		{
			var endpoint = new TrainingApi() { ApiKey = PrivateKeys.TrainingKey };

			Iteration latestIteration = await GetLatestIteration(endpoint);

			Export latestModelExport = await ExportLatestModel(endpoint, latestIteration);

			var model = await DownloadModel(latestModelExport);

		}

		private static async Task<object> DownloadModel(Export modelExport)
		{
			var path = modelExport.DownloadUri;

			var response = await visionClient.GetAsync(path);

			return response;
		}

		private static async Task<Export> ExportLatestModel(TrainingApi endpoint, Iteration latestIteration)
		{
			string modelType = "";
			switch (Device.RuntimePlatform)
			{
				case "Android":
					modelType = "tensorflow";
					break;
				case "iOS":
					modelType = "coreml";
					break;
				default:
					throw new NotSupportedException("Platform not supported.");
			}

			var model = await endpoint.ExportIterationAsync(PrivateKeys.ProjectId, latestIteration.Id, modelType);
			return model;
		}

		private static async Task<Iteration> GetLatestIteration(TrainingApi endpoint)
		{
			var iterations = await endpoint.GetIterationsAsync(PrivateKeys.ProjectId);
			var latestIteration = iterations.OrderByDescending(x => x.LastModified).FirstOrDefault();

			if (latestIteration == null)
			{
				throw new ArgumentNullException(nameof(latestIteration), "No trained project found.");
			}

			return latestIteration;
		}

		public static void UploadPartImage(byte[] imageBytes)
		{
			//todo: dynamic part tagging system
		}

		public static async Task UploadPartBatch(PVPartBatch batch)
		{

		}

		public async static Task<bool> UploadHandGestureImage(GestureCommand command, Stream stream)
		{
			return await UploadImage(command.AsVisionTagId(), stream);
		}

		private async static Task<bool> UploadImage(string tags, Stream stream)
		{
			var endpoint = new TrainingApi() { ApiKey = PrivateKeys.TrainingKey };

			var tagList = await endpoint.GetTagsAsync(PrivateKeys.ProjectId);

			ImageCreateSummary result = await endpoint.CreateImagesFromDataAsync(PrivateKeys.ProjectId, stream, new List<string>() { tags });

			stream.Dispose();

			LogResponse(result);
			return result.IsBatchSuccessful;
		}

		private static void LogResponse(ImageCreateSummary summary)
		{
			var message = "Azure CV Image Upload: ";
			message = message + (summary.IsBatchSuccessful ? "Success" : "Failure");
		}

		private static void LogResponse<T>(HttpOperationResponse response)
		{
			string message = "";

			switch (response)
			{
				case HttpOperationResponse<ImageCreateSummary> operationSummary:
					message = "Azure CV Image Upload: ";
					message = message + (operationSummary.Body.IsBatchSuccessful ? "Success" : "Failure");

					break;
				default:
					message = "Unrecognized Operation";
					break;
			}

			System.Console.WriteLine(message);

			if (!response.Response.IsSuccessStatusCode)
			{
				System.Console.WriteLine($"HTTP Code {response.Response.StatusCode}: {response.Response.ReasonPhrase}.");
			}
		}
	}
}
