import { Icon } from "../ui/Icon";

export function HeroSection() {
  return (
    <section aria-labelledby="hero-title" className="hero page-shell">
      <div className="hero-copy">
        <span className="eyebrow">
          <Icon name="sparkle" /> The art of getting dressed
        </span>
        <h1 id="hero-title">
          Find your fit
          <br />
          for <em>every day.</em>
        </h1>
        <p>
          Thoughtful layers, everyday essentials, and the little details that
          make getting dressed feel like you.
        </p>
        <div className="hero-actions">
          <a className="button button--dark" href="#shop">
            Shop the collection <Icon name="arrow-right" />
          </a>
          <a className="underlined-link" href="#story">
            Our point of view
          </a>
        </div>
        <div className="hero-note">
          <div className="avatar-stack" aria-hidden="true">
            <span>J</span>
            <span>M</span>
            <span>A</span>
          </div>
          <p>
            <strong>Made for your everyday</strong>
            <span>Pieces to wear on repeat</span>
          </p>
        </div>
      </div>

      <div className="hero-visual">
        <img
          alt="A fashion editorial featuring a dark, tailored everyday coat"
          className="hero-image"
          fetchPriority="high"
          src="/images/fashion/editorial-look.jpg"
        />
        <div className="hero-image-wash" />
        <div className="hero-caption">
          <span>01 / A considered wardrobe</span>
          <span>The Forma edit</span>
        </div>
        <div className="hero-product-note">
          <span className="note-dot" />
          <span>Everyday, with intention</span>
          <Icon name="arrow-right" />
        </div>
        <span aria-hidden="true" className="hero-side-note">
          FIND YOUR EVERYDAY
        </span>
      </div>
    </section>
  );
}
