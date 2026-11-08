import { Icon } from "../ui/Icon";

export function StorySection() {
  return (
    <section aria-labelledby="story-heading" className="story-section page-shell" id="story">
      <div className="story-image">
        <img
          alt="A curated rail of versatile jackets, knits, and everyday clothing"
          loading="lazy"
          src="/images/fashion/casual-look.jpg"
        />
        <span className="story-image-caption">Layers for the everyday.</span>
      </div>
      <div className="story-copy">
        <span className="eyebrow">A note from Forma</span>
        <h2 id="story-heading">
          A wardrobe that
          <br />
          feels like <em>you.</em>
        </h2>
        <p>
          From everyday essentials to layers worth keeping, we look for pieces
          that bring ease and confidence to getting dressed.
        </p>
        <a className="text-button" href="#shop">
          Explore the collection <Icon name="arrow-right" />
        </a>
        <span aria-hidden="true" className="story-stamp">
          FORM
          <br />
          FOLLOWS
          <br />
          FEELING
        </span>
      </div>
    </section>
  );
}
