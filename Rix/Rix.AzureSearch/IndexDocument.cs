using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rix.AzureSearch
{
    [SerializePropertyNamesAsCamelCase]
    public class IndexDocument
    {
        [Key]
        [IsRetrievable(true)]
        public string Id { get; set; }
        [IsRetrievable(true)]
        public string Content { get; set; }
    }
}
