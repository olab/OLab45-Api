using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLabWebAPI.Dto
{
    public partial class MapVpdTypesDto
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
    }
}
