import { Icon } from "../ui/Icon";

export function AnnouncementBar() {
  return (
    <div className="announcement-bar">
      <span>
        A little something for your wardrobe <strong>— free shipping over $100</strong>
      </span>
      <a href="/#shop">
        Discover the edit <Icon name="arrow-right" />
      </a>
    </div>
  );
}
