using AM.Services;
using AM.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web
{
    public class AutoMapperConfig
    {
        public static void ConfigureMappings()
        {
            Mapper.Initialize(cfg =>
           {
               cfg.CreateMap<MailPieces, SearchViewModel>()
                   .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                   .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.DocTypes != null ? (s.DocTypes.ShippingServiceType != null ? s.DocTypes.ShippingServiceType : "") : null))
                   .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null));
           });
        }
    }
}