using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Xamarin.Forms;
namespace PartVision.Standard
{
	public class UntaggedProduct
	{
		public UntaggedProduct()
		{

		}

		public List<Image> Images { get; set; } = new List<Image>();
	}

	public class TaggingGalleryViewModel : BaseViewModel
	{
		public ObservableCollection<UntaggedProduct> UntaggedProducts { get; set; }

		public TaggingGalleryViewModel()
		{

		}

		public void GetUntaggedSets()
		{

		}

		public void GetUntaggedImages(string setId)
		{

		}
	}
}
