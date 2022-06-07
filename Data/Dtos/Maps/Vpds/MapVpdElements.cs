using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLabWebAPI.Dto
{
    public partial class MapVpdElementsDto
    {
        public uint Id { get; set; }
        public uint VpdId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
