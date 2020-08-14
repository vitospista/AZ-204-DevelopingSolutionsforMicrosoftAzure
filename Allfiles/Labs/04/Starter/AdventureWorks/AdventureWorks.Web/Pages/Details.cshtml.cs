using AdventureWorks.Context;
using AdventureWorks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AdventureWorks.Web.Pages
{
    public class Details : PageModel
    {
        private readonly IAdventureWorksProductContext _productContext;

        public Details(IAdventureWorksProductContext productContext)
        {
            _productContext = productContext;
        }

        [BindProperty(SupportsGet = true)]
        public Model Model { get; set; }

        [BindProperty, Required(ErrorMessage = "Please select a product.")]
        public Guid SelectedProductId { get; set; }

        public async Task OnGetAsync(Guid id, string category)
        {
            this.Model = await _productContext.FindModelAsync(id, category);
        }

        public async Task OnPostAsync()
        {
            var product = await _productContext.FindProductAsync(SelectedProductId);
            await this.OnGetAsync(this.Model.id, this.Model.Category);
        }
    }
}