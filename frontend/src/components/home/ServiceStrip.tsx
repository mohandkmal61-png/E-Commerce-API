import { Icon } from "../ui/Icon";

const services = [
  {
    title: "Made with intention",
    body: "Good materials. Better choices.",
    icon: "sparkle" as const,
  },
  {
    title: "A little extra care",
    body: "Free shipping over $100.",
    icon: "check" as const,
  },
  {
    title: "Here if you need us",
    body: "Real people, ready to help.",
    icon: "arrow-right" as const,
  },
];

export function ServiceStrip() {
  return (
    <section aria-label="The Forma difference" className="service-strip">
      <div className="page-shell service-strip-inner">
        {services.map((service) => (
          <div className="service-item" key={service.title}>
            <span className="service-icon">
              <Icon name={service.icon} />
            </span>
            <span>
              <strong>{service.title}</strong>
              <small>{service.body}</small>
            </span>
          </div>
        ))}
      </div>
    </section>
  );
}
