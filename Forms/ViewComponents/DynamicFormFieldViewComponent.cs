using Forms.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.ViewComponents
{
    public class DynamicFormFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DynamicForm form, DynamicFormField field)
        {
            var model = new Tuple<DynamicForm, DynamicFormField>( form, field );
            return View(model);
        }
    }
}
