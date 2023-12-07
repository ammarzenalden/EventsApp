﻿namespace Events.Dto
{
    public class EventDto
    {
        public string? EventName { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? EventDate { get; set; }
        public int Available { get; set; }
    }
}
