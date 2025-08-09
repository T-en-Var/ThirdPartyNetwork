// src/lib/cloudinary-client.ts
import { scale, fill } from '@cloudinary/url-gen/actions/resize';
import { auto as fmtAuto } from '@cloudinary/url-gen/qualifiers/format';
import { format, quality } from '@cloudinary/url-gen/actions/delivery';
import { Effect } from '@cloudinary/url-gen/actions/effect';
import { Cloudinary } from '@cloudinary/url-gen';
import { CLOUDINARY_CLOUD_NAME } from '@/config';

export const cld = new Cloudinary({
  cloud: { cloudName: CLOUDINARY_CLOUD_NAME },
});

type ImageOpts = {
  width?: number;
  height?: number;
  crop?: 'scale' | 'fill';
  quality?: number | 'auto';
  format?: 'auto' | 'webp' | 'jpg' | 'png' | 'avif';
  dpr?: number | 'auto';
  radius?: number;
};

type SrcSetOpts = Omit<ImageOpts, 'width' | 'height'> & { widths?: number[] };

const DEFAULT_WIDTHS = [320, 480, 768, 1024, 1440, 1920];

/**
 * Generate an image URL with common options.
 */
export function imageUrl(publicId: string, opts: ImageOpts = {}) {
  const {
    width,
    height,
    crop = 'scale',
    quality: q = 'auto',
    format: f = 'auto',
    dpr,
    radius,
  } = opts;
  void f

  const img = cld.image(publicId);

  // resize
  if (crop === 'fill' && width && height) {
    img.resize(fill().width(width).height(height));
  } else if (width) {
    img.resize(scale().width(width));
  }

  // delivery
  img.delivery(format(fmtAuto())); // Instead of img.format(auto())
  if (q === 'auto') {
    img.delivery(quality('auto'));
  } else if (typeof q === 'number') {
    img.delivery(quality(q));
  }

  if (dpr) {
    img.setDeliveryType(`dpr_${dpr}`);
  }

  if (radius) {
    // lazy way without importing roundCorners
    // You can import roundCorners/byRadius for stricter typing:
    img.addTransformation(`r_${radius}`);
  }

  return img.toURL();
}

/**
 * Build a responsive srcset string (URLs at multiple widths).
 */
export function buildSrcSet(
  publicId: string,
  { widths = DEFAULT_WIDTHS, ...opts }: SrcSetOpts = {}
) {
  const entries = widths.map((w) => {
    const url = imageUrl(publicId, { ...opts, width: w });
    return `${url} ${w}w`;
  });
  return entries.join(', ');
}

/**
 * Tiny blurred placeholder (LQIP) for shimmer/skeleton UIs.
 */

export function placeholderBlur(publicId: string) {
  const img = cld.image(publicId);
  img
    .resize(scale().width(20))            // Tiny width for fast load
    .delivery(quality(1))                 // Lowest quality
    .delivery(format(fmtAuto()))          // Auto format
    .effect(Effect.blur(1000));           // Blur effect

  return img.toURL();
}

