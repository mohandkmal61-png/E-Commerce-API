import { Icon } from "../ui/Icon";

const footerLinks = [
  { title: "Explore", links: ["Shop all", "New arrivals", "Best sellers", "The journal"] },
  { title: "Help", links: ["Contact us", "Shipping & returns", "FAQs", "Care guide"] },
  { title: "Follow along", links: ["Instagram", "Pinterest", "Our story"] },
];

export function Footer() {
  return (
    <footer className="site-footer">
      <div className="page-shell">
        <div className="footer-main">
          <div className="footer-brand">
            <a className="wordmark wordmark--footer" href="#">
              forma<span>.</span>
            </a>
            <p>Considered things for a life well lived.</p>
            <a className="footer-contact" href="mailto:hello@forma.example">
              Say hello <Icon name="arrow-right" />
            </a>
          </div>

          {footerLinks.map((group) => (
            <div className="footer-column" key={group.title}>
              <h3>{group.title}</h3>
              {group.links.map((link) => (
                <a href="#shop" key={link}>
                  {link}
                </a>
              ))}
            </div>
          ))}
        </div>

        <div className="footer-bottom">
          <span>© {new Date().getFullYear()} Forma Studio. All rights reserved.</span>
          <div>
            <a href="#privacy">Privacy</a>
            <a href="#terms">Terms</a>
          </div>
          <span className="footer-note">Made for the everyday.</span>
        </div>
      </div>
    </footer>
  );
}
