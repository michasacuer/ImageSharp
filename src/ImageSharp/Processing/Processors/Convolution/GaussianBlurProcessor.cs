﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Processors.Convolution
{
    /// <summary>
    /// Applies Gaussian blur processing to the image.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class GaussianBlurProcessor<TPixel> : ImageProcessor<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The maximum size of the kernel in either direction.
        /// </summary>
        private readonly int kernelSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlurProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="sigma">The 'sigma' value representing the weight of the blur.</param>
        public GaussianBlurProcessor(float sigma = 3F)
            : this(sigma, (int)MathF.Ceiling(sigma * 3))
        {
            // Kernel radius is calculated using the minimum viable value.
            // http://chemaguerra.com/gaussian-filter-radius/
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlurProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="radius">
        /// The 'radius' value representing the size of the area to sample.
        /// </param>
        public GaussianBlurProcessor(int radius)
            : this(radius / 3F, radius)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlurProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="sigma">
        /// The 'sigma' value representing the weight of the blur.
        /// </param>
        /// <param name="radius">
        /// The 'radius' value representing the size of the area to sample.
        /// This should be at least twice the sigma value.
        /// </param>
        public GaussianBlurProcessor(float sigma, int radius)
        {
            this.kernelSize = (radius * 2) + 1;
            this.Sigma = sigma;
            this.KernelX = this.CreateGaussianKernel();
            this.KernelY = this.KernelX.Transpose();
        }

        /// <summary>
        /// Gets the sigma value representing the weight of the blur
        /// </summary>
        public float Sigma { get; }

        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        public DenseMatrix<float> KernelX { get; }

        /// <summary>
        /// Gets the vertical gradient operator.
        /// </summary>
        public DenseMatrix<float> KernelY { get; }

        /// <inheritdoc/>
        protected override void OnFrameApply(ImageFrame<TPixel> source, Rectangle sourceRectangle, Configuration configuration)
            => new Convolution2PassProcessor<TPixel>(this.KernelX, this.KernelY).Apply(source, sourceRectangle, configuration);

        /// <summary>
        /// Create a 1 dimensional Gaussian kernel using the Gaussian G(x) function
        /// </summary>
        /// <returns>The <see cref="DenseMatrix{T}"/></returns>
        private DenseMatrix<float> CreateGaussianKernel()
        {
            int size = this.kernelSize;
            float weight = this.Sigma;
            var kernel = new DenseMatrix<float>(size, 1);

            float sum = 0F;
            float midpoint = (size - 1) / 2F;

            for (int i = 0; i < size; i++)
            {
                float x = i - midpoint;
                float gx = ImageMaths.Gaussian(x, weight);
                sum += gx;
                kernel[0, i] = gx;
            }

            // Normalize kernel so that the sum of all weights equals 1
            for (int i = 0; i < size; i++)
            {
                kernel[0, i] /= sum;
            }

            return kernel;
        }
    }
}