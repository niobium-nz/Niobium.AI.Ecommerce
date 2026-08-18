import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import {
  MAX_INITIAL_TESTIMONIAL_COUNT,
  Testimonials,
  chooseInitialTestimonialCount,
  type Testimonial,
} from "../../components/sections/testimonials";

const testimonials: Testimonial[] = Array.from({ length: 10 }, (_, index) => ({
  name: `Customer ${index + 1}`,
  testimonial: `Exact supplied testimonial ${index + 1}.`,
}));

describe("chooseInitialTestimonialCount", () => {
  it.each([
    [-1, 0],
    [0, 0],
    [3, 3],
    [6, 6],
    [7, 4],
    [9, 4],
    [10, MAX_INITIAL_TESTIMONIAL_COUNT],
  ])("chooses an accessible initial count for %s testimonials", (total, expected) => {
    expect(chooseInitialTestimonialCount(total)).toBe(expected);
  });
});

describe("Testimonials", () => {
  it("renders every supplied testimonial after load-more actions without rewriting text", () => {
    const { container } = render(<Testimonials testimonials={testimonials} />);
    expect(container.querySelectorAll('[data-testimonial="true"]')).toHaveLength(6);

    fireEvent.click(screen.getByRole("button", { name: /load more testimonials/i }));

    expect(container.querySelectorAll('[data-testimonial="true"]')).toHaveLength(testimonials.length);
    expect(screen.queryByRole("button", { name: /load more testimonials/i })).not.toBeInTheDocument();
    for (const item of testimonials) {
      expect(screen.getByText(item.testimonial)).toBeInTheDocument();
      expect(screen.getByText(item.name)).toBeInTheDocument();
    }
  });

  it("shows all small sets without a load-more control", () => {
    render(<Testimonials testimonials={testimonials.slice(0, 3)} />);
    expect(screen.queryByRole("button", { name: /load more testimonials/i })).not.toBeInTheDocument();
  });
});
