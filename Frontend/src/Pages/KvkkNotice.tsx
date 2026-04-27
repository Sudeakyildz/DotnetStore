import { Link } from 'react-router-dom';

const KvkkNotice = () => {
  return (
    <div className="min-vh-100 bg-light py-5">
      <div className="container" style={{ maxWidth: 720 }}>
        <div className="card shadow-sm">
          <div className="card-body p-4 p-md-5">
            <h1 className="h4 mb-3">Kişisel verilerin korunması (KVKK / Aydınlatma)</h1>
            <p className="text-muted small mb-4">
              Bu yönetim paneli, staj kapsamındaki mağaza operasyonları için kullanılmaktadır. Giriş yaparak
              kimlik doğrulama bilgilerinizin (e-posta, oturum günlükleri: ilk ve son giriş zamanları) işlenmesine
              onay vermiş olursunuz. Veriler yalnızca yetkili personel tarafından, görev tanımıyla sınırlı şekilde
              kullanılır; teknik ve idari tedbirlerle korunur.
            </p>
            <p className="small mb-0">
              Sorularınız için veri sorumlusuna başvurun. Bu metin bilgilendirme amaçlıdır; ayrıntılı politika
              işvereninizle paylaşılabilir.
            </p>
            <div className="mt-4">
              <Link to="/" className="btn btn-outline-secondary btn-sm">
                Girişe dön
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default KvkkNotice;
