using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Ecm.CustomControl
{
    public class AnimatedImage : Image
    {
        static AnimatedImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedImage), new FrameworkPropertyMetadata(typeof(AnimatedImage)));
        }

        public RepeatBehavior AnimationRepeatBehavior
        {
            get
            {
                return (RepeatBehavior)GetValue(AnimationRepeatBehaviorProperty);
            }
            set
            {
                SetValue(AnimationRepeatBehaviorProperty, value);
            }
        }

        public BitmapImage BitmapSource
        {
            get
            {
                return (BitmapImage)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public int FrameIndex
        {
            get
            {
                return (int)GetValue(FrameIndexProperty);
            }
            set
            {
                SetValue(FrameIndexProperty, value);
            }
        }

        public List<BitmapFrame> Frames { get; private set; }

        public Uri UriSource
        {
            get
            {
                return (Uri)GetValue(UriSourceProperty);
            }
            set
            {
                SetValue(UriSourceProperty, value);
            }
        }

        private Int32Animation Animation { get; set; }

        private bool IsAnimationWorking { get; set; }

        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            ClearAnimation();
            BitmapImage source;
            BitmapDecoder decoder;

            if (e.NewValue is Uri)
            {
                source = new BitmapImage();
                source.BeginInit();
                source.UriSource = e.NewValue as Uri;
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.EndInit();
            }
            else if (e.NewValue is BitmapImage)
            {
                source = e.NewValue as BitmapImage;
            }
            else
            {
                return;
            }

            if (source.StreamSource != null)
            {
                decoder = BitmapDecoder.Create(source.StreamSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
            }
            else if (source.UriSource != null)
            {
                decoder = BitmapDecoder.Create(source.UriSource, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
            }
            else
            {
                return;
            }

            if (decoder.Frames.Count == 1)
            {
                Source = decoder.Frames[0];
                return;
            }

            Frames = decoder.Frames.ToList();

            PrepareAnimation();
        }

        private static void ChangingFrameIndex(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            AnimatedImage animatedImage = dp as AnimatedImage;

            if (animatedImage == null || !animatedImage.IsAnimationWorking)
            {
                return;
            }

            int frameIndex = (int)e.NewValue;
            (animatedImage).Source = animatedImage.Frames[frameIndex];
            animatedImage.InvalidateVisual();
        }

        /// <summary>
        ///   Handles changes to the Source property.
        /// </summary>
        private static void OnSourceChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedImage)dp).OnSourceChanged(e);
        }

        private void ClearAnimation()
        {
            if (Animation != null)
            {
                BeginAnimation(FrameIndexProperty, null);
            }

            IsAnimationWorking = false;
            Animation = null;
            Frames = null;
        }

        private void PrepareAnimation()
        {
            Animation = new Int32Animation(
                0,
                Frames.Count - 1,
                new Duration(
                    new TimeSpan(
                        0,
                        0,
                        0,
                        Frames.Count / 10,
                        (int)((Frames.Count / 10.0 - Frames.Count / 10) * 1000))))
                { RepeatBehavior = RepeatBehavior.Forever };

            Source = Frames[0];
            BeginAnimation(FrameIndexProperty, Animation);
            IsAnimationWorking = true;
        }

        public static readonly DependencyProperty AnimationRepeatBehaviorProperty =
            DependencyProperty.Register(
                "AnimationRepeatBehavior", typeof(RepeatBehavior), typeof(AnimatedImage), new PropertyMetadata(null));

        public static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register(
            "FrameIndex", typeof(int), typeof(AnimatedImage), new UIPropertyMetadata(0, ChangingFrameIndex));

        public static new readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(BitmapImage),
            typeof(AnimatedImage),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnSourceChanged));

        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.Register(
            "UriSource",
            typeof(Uri),
            typeof(AnimatedImage),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnSourceChanged));
    }
}