using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class NameEntity : TableEntity
    {
        public string Field { get; set; }
        public string Id { get; set; }
    }
}
