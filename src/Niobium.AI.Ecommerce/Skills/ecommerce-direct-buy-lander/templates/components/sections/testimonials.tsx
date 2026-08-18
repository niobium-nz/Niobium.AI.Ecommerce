"use client";

import { useState } from "react";

export type Testimonial = {
  name: string;
  testimonial: string;
  city?: string;
  location?: string;
  rating?: number;
  image_url?: string;
  video_url?: string;
  media_ratio?: string;
};

export const MIN_INITIAL_TESTIMONIAL_COUNT = 3;
export const MAX_INITIAL_TESTIMONIAL_COUNT = 6;

export function chooseInitialTestimonialCount(total: number): number {
  if (!Number.isSafeInteger(total) || total <= 0) return 0;
  if (total <= MAX_INITIAL_TESTIMONIAL_COUNT) return total;
  if (total <= 9) return 4;
  return MAX_INITIAL_TESTIMONIAL_COUNT;
}

export function Testimonials({ testimonials }: { testimonials: readonly Testimonial[] }) {
  const initialCount = chooseInitialTestimonialCount(testimonials.length);
  const [visibleCount, setVisibleCount] = useState(initialCount);
  const visibleTestimonials = testimonials.slice(0, visibleCount);
  const remainingCount = testimonials.length - visibleTestimonials.length;
  const batchSize = Math.max(MIN_INITIAL_TESTIMONIAL_COUNT, initialCount);

  return (
    <section
      aria-labelledby="customer-feedback-heading"
      data-testimonials="true"
      data-testimonials-total={testimonials.length}
      data-testimonials-visible={visibleTestimonials.length}
    >
      <h2 id="customer-feedback-heading">What customers say</h2>
      <div aria-live="polite">
        {visibleTestimonials.map((item, index) => (
          <article
            data-testimonial="true"
            data-testimonial-index={index}
            key={`${item.name}-${index}`}
          >
            <blockquote>{item.testimonial}</blockquote>
            <p>
              <strong>{item.name}</strong>
              {item.city ? `, ${item.city}` : item.location ? `, ${item.location}` : null}
            </p>
          </article>
        ))}
      </div>
      {remainingCount > 0 ? (
        <button
          type="button"
          data-load-more-testimonials="true"
          onClick={() => setVisibleCount((count) => Math.min(testimonials.length, count + batchSize))}
        >
          Load more testimonials ({remainingCount})
        </button>
      ) : null}
    </section>
  );
}
