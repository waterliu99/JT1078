﻿using JT1078.Flv.Enums;
using JT1078.Flv.Extensions;
using JT1078.Flv.H264;
using JT1078.Flv.MessagePack;
using JT1078.Flv.Metadata;
using JT1078.Protocol;
using JT1078.Protocol.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JT1078.Flv.Test")]
namespace JT1078.Flv
{
    public class FlvEncoder
    {
        public static readonly byte[] VideoFlvHeaderBuffer;
        private static readonly Flv.H264.H264Decoder H264Decoder;
        private static readonly ConcurrentDictionary<string, SPSInfo> VideoSPSDict;
        private static readonly ConcurrentDictionary<string, FlvFrameInfo> FlvFrameInfoDict;
        internal static readonly ConcurrentDictionary<string, (uint PreviousTagSize,byte[] Buffer,bool Changed)> FirstFlvFrameCache;
        private readonly ILogger logger;
        static FlvEncoder()
        {
            FlvHeader VideoFlvHeader = new FlvHeader(true, false);
            VideoFlvHeaderBuffer = VideoFlvHeader.ToArray().ToArray();
            VideoSPSDict = new ConcurrentDictionary<string, SPSInfo>(StringComparer.OrdinalIgnoreCase);
            FlvFrameInfoDict = new ConcurrentDictionary<string, FlvFrameInfo>(StringComparer.OrdinalIgnoreCase);
            FirstFlvFrameCache = new ConcurrentDictionary<string, (uint PreviousTagSize, byte[] Buffer, bool Changed)>(StringComparer.OrdinalIgnoreCase);
            H264Decoder = new Flv.H264.H264Decoder();
        }
        public FlvEncoder()
        {

        }
        public FlvEncoder(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("FlvEncoder");
        }
        public byte[] CreateScriptTagFrame(int width, int height, double frameRate = 25d)
        {
            byte[] buffer = FlvArrayPool.Rent(1024);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body script tag
                //flv body tag header
                FlvTags flvTags = new FlvTags();
                flvTags.Type = TagType.ScriptData;
                flvTags.Timestamp = 0;
                flvTags.TimestampExt = 0;
                flvTags.StreamId = 0;
                //flv body tag body
                flvTags.DataTagsData = new Amf3();
                flvTags.DataTagsData.Amf3Metadatas = new List<IAmf3Metadata>();
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_Duration
                {
                    Value = 0d
                });
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_VideoDataRate
                {
                    Value = 0d
                });
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_VideoCodecId
                {
                    Value = 7d
                });
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_FrameRate
                {
                    Value = frameRate
                });
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_Width
                {
                    Value = width
                });
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_Height
                {
                    Value = height
                });
                flvMessagePackWriter.WriteFlvTag(flvTags);
                return flvMessagePackWriter.FlushAndGetArray();
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public byte[] CreateVideoTag0Frame(byte[] spsRawData, byte[] ppsRawData, SPSInfo spsInfo)
        {
            byte[] buffer = FlvArrayPool.Rent(2048);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body video tag
                //flv body tag header
                FlvTags flvTags = new FlvTags();
                flvTags.Type = TagType.Video;
                flvTags.Timestamp = 0;
                flvTags.TimestampExt = 0;
                flvTags.StreamId = 0;
                //flv body tag body
                flvTags.VideoTagsData = new VideoTags();
                flvTags.VideoTagsData.FrameType = FrameType.KeyFrame;
                flvTags.VideoTagsData.VideoData = new AvcVideoPacke();
                flvTags.VideoTagsData.VideoData.AvcPacketType = AvcPacketType.SequenceHeader;
                flvTags.VideoTagsData.VideoData.CompositionTime = 0;
                AVCDecoderConfigurationRecord aVCDecoderConfigurationRecord = new AVCDecoderConfigurationRecord();
                aVCDecoderConfigurationRecord.AVCProfileIndication = spsInfo.profileIdc;
                aVCDecoderConfigurationRecord.ProfileCompatibility = (byte)spsInfo.profileCompat;
                aVCDecoderConfigurationRecord.AVCLevelIndication = spsInfo.levelIdc;
                aVCDecoderConfigurationRecord.NumOfPictureParameterSets = 1;
                aVCDecoderConfigurationRecord.PPSBuffer = ppsRawData;
                aVCDecoderConfigurationRecord.SPSBuffer = spsRawData;
                flvTags.VideoTagsData.VideoData.AVCDecoderConfiguration = aVCDecoderConfigurationRecord;
                flvMessagePackWriter.WriteFlvTag(flvTags);
                return flvMessagePackWriter.FlushAndGetArray();
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public byte[] CreateSecondVideoTag0Frame(byte[] spsRawData, byte[] ppsRawData, SPSInfo spsInfo)
        {
            byte[] buffer = FlvArrayPool.Rent(2048);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body video tag
                //flv body tag header
                FlvTags flvTags = new FlvTags();
                flvTags.Type = TagType.Video;
                flvTags.Timestamp =0;
                flvTags.TimestampExt = 0;
                flvTags.StreamId = 0;
                //flv body tag body
                flvTags.VideoTagsData = new VideoTags();
                flvTags.VideoTagsData.FrameType = FrameType.KeyFrame;
                flvTags.VideoTagsData.VideoData = new AvcVideoPacke();
                flvTags.VideoTagsData.VideoData.AvcPacketType = AvcPacketType.SequenceHeader;
                flvTags.VideoTagsData.VideoData.CompositionTime = 0;
                AVCDecoderConfigurationRecord aVCDecoderConfigurationRecord = new AVCDecoderConfigurationRecord();
                aVCDecoderConfigurationRecord.AVCProfileIndication = spsInfo.profileIdc;
                aVCDecoderConfigurationRecord.ProfileCompatibility = (byte)spsInfo.profileCompat;
                aVCDecoderConfigurationRecord.AVCLevelIndication = spsInfo.levelIdc;
                aVCDecoderConfigurationRecord.NumOfPictureParameterSets = 1;
                aVCDecoderConfigurationRecord.PPSBuffer = ppsRawData;
                aVCDecoderConfigurationRecord.SPSBuffer = spsRawData;
                flvTags.VideoTagsData.VideoData.AVCDecoderConfiguration = aVCDecoderConfigurationRecord;
                flvMessagePackWriter.WriteFlvTag(flvTags);
                return flvMessagePackWriter.FlushAndGetArray();
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public byte[] CreateVideoTagOtherFrame(FlvFrameInfo flvFrameInfo, H264NALU nALU, H264NALU sei)
        {
            byte[] buffer = FlvArrayPool.Rent(65535);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body video tag
                //flv body tag header
                FlvTags flvTags = new FlvTags();
                flvTags.Type = TagType.Video;
                //pts
                flvTags.Timestamp = flvFrameInfo.Interval;
                flvTags.TimestampExt = 0;
                flvTags.StreamId = 0;
                //flv body tag body
                flvTags.VideoTagsData = new VideoTags();
                flvTags.VideoTagsData.VideoData = new AvcVideoPacke();
                flvTags.VideoTagsData.VideoData.AvcPacketType = AvcPacketType.Raw;
                if (nALU.NALUHeader.NalUnitType == 5 || nALU.DataType == JT1078DataType.视频I帧)
                {
                    flvTags.VideoTagsData.FrameType = FrameType.KeyFrame;
                }
                else
                {
                    flvTags.VideoTagsData.FrameType = FrameType.InterFrame;
                }
                if(flvFrameInfo.DataType== JT1078DataType.视频I帧)
                {
                    //cts
                    flvTags.VideoTagsData.VideoData.CompositionTime = nALU.LastIFrameInterval;
                }
                else
                {
                    //cts
                    flvTags.VideoTagsData.VideoData.CompositionTime = nALU.LastFrameInterval;
                }
                flvTags.VideoTagsData.VideoData.MultiData = new List<byte[]>();
                flvTags.VideoTagsData.VideoData.MultiData.Add(nALU.RawData);
                //忽略sei
                //if (sei != null && sei.RawData != null && sei.RawData.Length > 0)
                //{
                //    flvTags.VideoTagsData.VideoData.MultiData.Add(sei.RawData);
                //}
                flvMessagePackWriter.WriteFlvTag(flvTags);
                return flvMessagePackWriter.FlushAndGetArray();
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public byte[] CreateFlvFrame(List<H264NALU> nALUs,int minimumLength = 65535)
        {
            byte[] buffer = FlvArrayPool.Rent(minimumLength);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                H264NALU sps=null, pps=null, sei=null;
                foreach (var naln in nALUs)
                {
                    string key = naln.GetKey();
                    if (sps != null && pps != null)
                    {
                        var rawData = H264Decoder.DiscardEmulationPreventionBytes(sps.RawData);
                        ExpGolombReader h264GolombReader = new ExpGolombReader(rawData);
                        SPSInfo spsInfo = h264GolombReader.ReadSPS();
                        if (VideoSPSDict.TryGetValue(key, out var spsInfoCache))
                        {
                            //切换主次码流
                            //根据宽高来判断
                            if (spsInfoCache.height != spsInfo.height && spsInfoCache.width != spsInfo.width)
                            {
                                if (logger != null)
                                {
                                    if (logger.IsEnabled(LogLevel.Debug))
                                    {
                                        logger.LogDebug($"Cache:{spsInfoCache.height}-{spsInfoCache.width},Current:{spsInfo.height}-{spsInfo.width}");
                                    }
                                }
                                VideoSPSDict.TryUpdate(key, spsInfo, spsInfoCache);
                                if (FlvFrameInfoDict.TryGetValue(key, out FlvFrameInfo flvFrameInfo))
                                {
                                    flvFrameInfo.Timestamp = naln.Timestamp;
                                    if (logger != null)
                                    {
                                        if (logger.IsEnabled(LogLevel.Debug))
                                        {
                                            logger.LogDebug($"Cache:{spsInfoCache.height}-{spsInfoCache.width},Current:{spsInfo.height}-{spsInfo.width}");
                                        }
                                    }
                                    var secondFlvKeyFrame=CreateSecondFlvKeyFrame(sps.RawData, pps.RawData, spsInfo, flvFrameInfo);
                                    flvMessagePackWriter.WriteArray(secondFlvKeyFrame.Buffer);
                                    flvFrameInfo.PreviousTagSize = secondFlvKeyFrame.PreviousTagSize;
                                    FlvFrameInfoDict.TryUpdate(key, flvFrameInfo, flvFrameInfo);
                                    if(FirstFlvFrameCache.TryGetValue(key,out var firstFlvFrameCacche))
                                    {
                                        FirstFlvFrameCache.TryUpdate(key, (secondFlvKeyFrame.PreviousTagSize, secondFlvKeyFrame.Buffer, true), firstFlvFrameCacche);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var firstFlvKeyFrame = CreateFirstFlvKeyFrame(sps.RawData, pps.RawData, spsInfo);
                            flvMessagePackWriter.WriteArray(firstFlvKeyFrame.Buffer);
                            if (logger != null)
                            {
                                if (logger.IsEnabled(LogLevel.Debug))
                                {
                                    logger.LogDebug($"Current:{spsInfo.height}-{spsInfo.width}");
                                }
                            }
                            //cache PreviousTagSize
                            FlvFrameInfoDict.TryAdd(key, new FlvFrameInfo{
                                    PreviousTagSize = firstFlvKeyFrame.PreviousTagSize,
                                    Interval = (uint)(pps.Timestamp - sps.Timestamp),
                                    Timestamp= pps.Timestamp,
                                }
                            ); 
                            FirstFlvFrameCache.TryAdd(key, (firstFlvKeyFrame.PreviousTagSize, firstFlvKeyFrame.Buffer, false));      
                            VideoSPSDict.TryAdd(key, spsInfo);
                        }
                        sps = null;
                        pps = null;
                        continue;
                    }
                    if (logger != null)
                    {
                        if (logger.IsEnabled(LogLevel.Trace))
                        {
                            logger.LogTrace($"SIM:{naln.SIM},CH:{naln.LogicChannelNumber},{naln.DataType.ToString()},NalUnitType:{naln.NALUHeader.NalUnitType},RawData:{naln.RawData.ToHexString()}");
                        }
                    }
                    //7 8 6 5 1 1 1 1 7 8 6 5 1 1 1 1 1 7 8 6 5 1 1 1 1 1
                    switch (naln.NALUHeader.NalUnitType)
                    {
                        case 5:// IDR
                        case 1:// I/P/B
                            if (FlvFrameInfoDict.TryGetValue(key, out FlvFrameInfo  flvFrameInfo))
                            {
                                //当前的1078包与上一包1078的时间戳相减再进行累加
                                uint interval = (uint)(naln.Timestamp - flvFrameInfo.Timestamp);
                                flvFrameInfo.Interval += interval;
                                flvFrameInfo.Timestamp = naln.Timestamp;
                                // PreviousTagSize
                                flvMessagePackWriter.WriteUInt32(flvFrameInfo.PreviousTagSize);
                                // Data Tag Frame
                                var flvFrameBuffer = CreateVideoTagOtherFrame(flvFrameInfo, naln, sei);
                                flvMessagePackWriter.WriteArray(flvFrameBuffer);
                                flvFrameInfo.PreviousTagSize = (uint)flvFrameBuffer.Length;
                                flvFrameInfo.DataType = naln.DataType;
                                FlvFrameInfoDict.TryUpdate(key, flvFrameInfo, flvFrameInfo);
                            }
                            break;
                        case 7:// SPS
                            sps = naln;
                            break;
                        case 8:// PPS
                            pps = naln;
                            break;
                        case 6://SEI
                            sei = naln;
                            break;
                        default:
                            break;
                    }
                }
                return flvMessagePackWriter.FlushAndGetArray();
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public (byte[] Buffer, uint PreviousTagSize) CreateFirstFlvKeyFrame(byte[] spsRawData, byte[] ppsRawData, SPSInfo spsInfo)
        {
            byte[] buffer = FlvArrayPool.Rent(65535);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body PreviousTagSize awalys 0
                flvMessagePackWriter.WriteUInt32(0);
                //flv body script tag
                var scriptTagFrameBuffer= CreateScriptTagFrame(spsInfo.width, spsInfo.height);
                flvMessagePackWriter.WriteArray(scriptTagFrameBuffer);
                //flv script tag PreviousTagSize
                flvMessagePackWriter.WriteUInt32((uint)scriptTagFrameBuffer.Length);
                //flv body video tag 0
                var videoTagFrame0Buffer= CreateVideoTag0Frame(spsRawData, ppsRawData, spsInfo);
                flvMessagePackWriter.WriteArray(videoTagFrame0Buffer);
                uint videoTag0PreviousTagSize = (uint)videoTagFrame0Buffer.Length;
                return (flvMessagePackWriter.FlushAndGetArray(), videoTag0PreviousTagSize);
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public (byte[] Buffer,uint PreviousTagSize) CreateSecondFlvKeyFrame(byte[] spsRawData, byte[] ppsRawData, SPSInfo spsInfo, FlvFrameInfo flvFrameInfo)
        {
            byte[] buffer = FlvArrayPool.Rent(65535);
            try
            {
                FlvMessagePackWriter flvMessagePackWriter = new FlvMessagePackWriter(buffer);
                //flv body PreviousTagSize awalys 0
                //当首包的时候为 0
                //当切花主次码流的时候需要根据上一包的大小
                flvMessagePackWriter.WriteUInt32(flvFrameInfo.PreviousTagSize);
                //flv body script tag
                var scriptTagFrameBuffer = CreateScriptTagFrame(spsInfo.width, spsInfo.height);
                flvMessagePackWriter.WriteArray(scriptTagFrameBuffer);
                //flv script tag PreviousTagSize
                flvMessagePackWriter.WriteUInt32((uint)scriptTagFrameBuffer.Length);
                //flv body video tag 0
                var videoTagFrame0Buffer = CreateSecondVideoTag0Frame(spsRawData, ppsRawData, spsInfo);
                flvMessagePackWriter.WriteArray(videoTagFrame0Buffer);
                uint videoTag0PreviousTagSize = (uint)videoTagFrame0Buffer.Length;
                return (flvMessagePackWriter.FlushAndGetArray(), videoTag0PreviousTagSize);
            }
            finally
            {
                FlvArrayPool.Return(buffer);
            }
        }
        public byte[] CreateFlvFrame(JT1078Package package,int minimumLength = 65535)
        {
            var nalus = H264Decoder.ParseNALU(package);
            if (nalus == null || nalus.Count <= 0) return default;
            return CreateFlvFrame(nalus, minimumLength);
        }
        public byte[] GetFirstFlvFrame(string key,byte[] bufferFlvFrame)
        {
            if (FirstFlvFrameCache.TryGetValue(key, out var firstBuffer))
            {
                var length = firstBuffer.Buffer.Length + bufferFlvFrame.Length + VideoFlvHeaderBuffer.Length;
                byte[] buffer = FlvArrayPool.Rent(length);
                try
                {
                    Span<byte> tmp = buffer;
                    VideoFlvHeaderBuffer.CopyTo(tmp);
                    if (firstBuffer.Changed)
                    {
                        //新用户进来需要替换为首包的PreviousTagSize 0
                        BinaryPrimitives.WriteUInt32BigEndian(firstBuffer.Buffer, 0);
                        firstBuffer.Buffer.CopyTo(tmp.Slice(VideoFlvHeaderBuffer.Length));
                        //新用户进来需要替换为上一包的PreviousTagSize
                        BinaryPrimitives.WriteUInt32BigEndian(bufferFlvFrame, firstBuffer.PreviousTagSize);
                        bufferFlvFrame.CopyTo(tmp.Slice(VideoFlvHeaderBuffer.Length + firstBuffer.Buffer.Length));
                        return tmp.Slice(0, length).ToArray();
                    }
                    else
                    {
                        firstBuffer.Buffer.CopyTo(tmp.Slice(VideoFlvHeaderBuffer.Length));
                        //新用户进来需要替换为首包的PreviousTagSize
                        BinaryPrimitives.WriteUInt32BigEndian(bufferFlvFrame, firstBuffer.PreviousTagSize);
                        bufferFlvFrame.CopyTo(tmp.Slice(VideoFlvHeaderBuffer.Length + firstBuffer.Buffer.Length));
                        return tmp.Slice(0, length).ToArray();
                    }
                }
                finally
                {
                    FlvArrayPool.Return(buffer);
                }
             }
            return default;
        }
    }

    public class FlvFrameInfo
    {
        public uint PreviousTagSize { get; set; }
        public ulong Timestamp { get; set; }
        public uint Interval { get; set; }
        public JT1078DataType DataType { get; set; }
    }
}
