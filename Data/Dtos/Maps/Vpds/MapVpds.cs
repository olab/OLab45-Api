using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OLabWebAPI.Model;

namespace OLabWebAPI.Dto
{
    public partial class MapVpdsDto
    {
        public uint Id { get; set; }
        public uint MapId { get; set; }
        public uint VpdTypeId { get; set; }
        public virtual MapVpdTypes VpdType { get; set; }
    }
}
