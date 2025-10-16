using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ST.Application.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectList<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>().Select(e => new SelectListItem
            {
                Text = e.GetType()
                        .GetMember(e.ToString())
                        .FirstOrDefault()?
                        .GetCustomAttribute<DisplayAttribute>()?
                        .GetName() ?? e.ToString(),
                Value = Convert.ToInt32(e).ToString()
            }).ToList();
        }
    }
}