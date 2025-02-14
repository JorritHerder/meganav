﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cogworks.Meganav.Enums;
using Cogworks.Meganav.Helpers;
using Cogworks.Meganav.Models;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;

namespace Cogworks.Meganav.ValueConverters
{

    public class MeganavValueConverter : PropertyValueConverterBase
    {
        private bool RemoveNaviHideItems;

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditorAlias);
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object source, bool preview)
        {
            var sourceString = source?.ToString();

            if (string.IsNullOrWhiteSpace(sourceString))
            {
                return null;
            }

            var preValues = PreValueHelper.GetPreValues(propertyType.DataType.Id);

            if (preValues.ContainsKey("removeNaviHideItems"))
            {
                RemoveNaviHideItems = preValues["removeNaviHideItems"] == "1";
            }

            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<MeganavItem>>(sourceString);

                return BuildMenu(items);
            }
            catch (Exception ex)
            {
                Umbraco.Core.Composing.Current.Logger.Error<MeganavValueConverter>("Failed to convert Meganav", ex);
            }

            return null;
        }

        internal IEnumerable<MeganavItem> BuildMenu(IEnumerable<MeganavItem> items, int level = 0)
        {
            items = items.ToList();

            foreach (var item in items)
            {
                item.Level = level;

                // it's likely a content item
                if (item.Id > 0)
                {
                    var umbracoContent = Umbraco.Web.Composing.Current.UmbracoHelper.Content(item.Id);

                    if (umbracoContent != null)
                    {
                        // set item type
                        item.ItemType = ItemType.Content;

                        // skip item if umbracoNaviHide enabled
                        if (RemoveNaviHideItems && !umbracoContent.IsVisible())
                        {
                            continue;
                        }

                        // set content to node
                        item.Content = umbracoContent;

                        // set title to node name if no override is set
                        if (string.IsNullOrWhiteSpace(item.Title))
                        {
                            item.Title = umbracoContent.Name;
                        }
                    } 
                }

                // process child items
                if (item.Children.Any())
                {
                    var childLevel = item.Level + 1;

                    BuildMenu(item.Children, childLevel);
                }
            }

            // remove unpublished content items
            items = items.Where(x => x.Content != null || x.ItemType == ItemType.Link);

            return items;
        }
    }
}