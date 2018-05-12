using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartVision.Standard
{
	public class LiteDB : IDataStore<BaseModel>
	{
		private const string MLCollection = "";

		public LiteDB()
		{


		}

		public Task<bool> AddItemAsync(BaseModel item)
		{
			throw new NotImplementedException();
		}

		public Task<bool> DeleteItemAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<BaseModel> GetItemAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<BaseModel>> GetItemsAsync(bool forceRefresh = false)
		{
			throw new NotImplementedException();
		}

		public Task<bool> UpdateItemAsync(BaseModel item)
		{
			throw new NotImplementedException();
		}
	}
}
