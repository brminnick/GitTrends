namespace GitTrends;

class TrendsCarouselDataTemplateSelector(StarsTrendsView starsTrendsView, ViewsClonesTrendsView viewsClonesTrendsView) : DataTemplateSelector
{
	readonly DataTemplate _starsTrendsView = starsTrendsView;
	readonly DataTemplate _viewsClonesTrendsView = viewsClonesTrendsView;
	
	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var pageNumber = (int)item;

		return pageNumber switch
		{
			0 => _viewsClonesTrendsView,
			1 => _starsTrendsView,
			_ => throw new NotImplementedException()
		};
	}
}