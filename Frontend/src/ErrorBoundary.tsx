import { Component, type ErrorInfo, type ReactNode } from 'react';

type Props = { children: ReactNode };

type State = { error: Error | null };

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error(error, info.componentStack);
  }

  render() {
    if (this.state.error) {
      return (
        <div style={{ padding: 24, fontFamily: 'system-ui,sans-serif', maxWidth: 640, margin: '48px auto' }}>
          <h1 style={{ fontSize: 20, marginBottom: 12 }}>Uygulama hatası</h1>
          <pre
            style={{
              background: '#f5f5f5',
              padding: 12,
              overflow: 'auto',
              fontSize: 13,
              borderRadius: 8,
            }}
          >
            {this.state.error.message}
          </pre>
          <p style={{ marginTop: 16, color: '#555' }}>
            Sayfayı yenileyin. Geliştirme modunda tarayıcı konsolundaki (F12) kırmızı hatayı da kontrol edin.
          </p>
        </div>
      );
    }
    return this.props.children;
  }
}
