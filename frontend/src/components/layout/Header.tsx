import { useState } from "react";
import { AnnouncementBar } from "./AnnouncementBar";
import { Icon } from "../ui/Icon";
import { useBasket } from "../../contexts/BasketContext";
import { useAuth } from "../../contexts/AuthContext";

const links = [
  { label: "Shop", href: "/#shop" },
  { label: "Collections", href: "/#categories" },
  { label: "Our story", href: "/#story" },
];

export function Header() {
  const [menuOpen, setMenuOpen] = useState(false);
  const { itemCount, loading: basketLoading } = useBasket();
  const { user, logout } = useAuth();

  return (
    <>
      <AnnouncementBar />
      <header className="site-header">
        <div className="header-inner page-shell">
          <button
            aria-expanded={menuOpen}
            aria-label={menuOpen ? "Close navigation menu" : "Open navigation menu"}
            className="icon-button menu-toggle"
            onClick={() => setMenuOpen((open) => !open)}
            type="button"
          >
            <Icon name={menuOpen ? "close" : "menu"} />
          </button>

          <a aria-label="Forma home" className="wordmark" href="/">
            forma<span>.</span>
          </a>

          <nav
            aria-label="Main navigation"
            className={`main-nav${menuOpen ? " main-nav--open" : ""}`}
          >
            {links.map((link) => (
              <a href={link.href} key={link.label} onClick={() => setMenuOpen(false)}>
                {link.label}
              </a>
            ))}
          </nav>

          <div className="header-actions">
            <a aria-label="Browse products" className="icon-button search-link" href="/#shop">
              <Icon name="search" />
            </a>
            {user ? (
              <>
                <span className="header-user">{user.displayName || user.email}</span>
                <button className="header-auth-button" onClick={logout} type="button">
                  Sign out
                </button>
              </>
            ) : (
              <a className="header-auth-button" href="/login?returnUrl=%2Fbasket">
                Sign in
              </a>
            )}
            <a
              aria-label={`Shopping bag, ${itemCount} ${itemCount === 1 ? "item" : "items"}`}
              className="bag-link"
              href="/basket"
            >
              <Icon name="bag" />
              <span className="bag-label">Bag</span>
              <span aria-live="polite" className="bag-count">
                {basketLoading ? "…" : itemCount}
              </span>
            </a>
          </div>
        </div>
      </header>
    </>
  );
}
