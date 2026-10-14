using it_lab1_dbcs.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace it_lab1_dbcs.Test
{
    public class TableSortingTests
    {
        [Fact]
        public void SortByField_IntegerField_SortsAscendingAndDescendingCorrectly()
        {
            var table = new Table
            {
                Name = "Employees",
                Schema = new TableSchema
                {
                    Fields = new List<FieldScheme>
                    {
                        new FieldScheme { Name = "Age", Type = FieldType.Integer },
                        new FieldScheme { Name = "Name", Type = FieldType.String }
                    }
                },
                Rows = new ObservableCollection<TableRow>
                {
                    new TableRow { Values = new Dictionary<string, string> { { "Age", "30" }, { "Name", "Олег" } } },
                    new TableRow { Values = new Dictionary<string, string> { { "Age", "18" }, { "Name", "Анна" } } },
                    new TableRow { Values = new Dictionary<string, string> { { "Age", "25" }, { "Name", "Іван" } } }
                }
            };
            table.SortByField("Age", ascending: true);
            // Assert 1
            Assert.Equal("18", table.Rows[0]["Age"]);
            Assert.Equal("25", table.Rows[1]["Age"]);
            Assert.Equal("30", table.Rows[2]["Age"]);
            table.SortByField("Age", ascending: false);

            // Assert 2 
            Assert.Equal("30", table.Rows[0]["Age"]);
            Assert.Equal("25", table.Rows[1]["Age"]);
            Assert.Equal("18", table.Rows[2]["Age"]);
        }
    }
}