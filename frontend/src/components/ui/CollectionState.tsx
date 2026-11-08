import { Icon } from "./Icon";

type CollectionStateProps =
  | { kind: "loading"; message?: string }
  | { kind: "error"; message: string; onRetry?: () => void }
  | { kind: "empty"; message?: string };

export function CollectionState(props: CollectionStateProps) {
  if (props.kind === "loading") {
    return (
      <div className="collection-state" role="status">
        <span className="loading-mark" aria-hidden="true" />
        <p>{props.message ?? "Finding something lovely..."}</p>
      </div>
    );
  }

  if (props.kind === "error") {
    return (
      <div className="collection-state collection-state--error" role="alert">
        <span className="state-icon">
          <Icon name="close" />
        </span>
        <h3>We couldn’t load this collection.</h3>
        <p>{props.message}</p>
        {props.onRetry && (
          <button className="text-button" onClick={props.onRetry} type="button">
            Try again <Icon name="arrow-right" />
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="collection-state" role="status">
      <span className="state-icon">
        <Icon name="search" />
      </span>
      <h3>No pieces found just yet.</h3>
      <p>{props.message ?? "Try another search or explore a different category."}</p>
    </div>
  );
}
