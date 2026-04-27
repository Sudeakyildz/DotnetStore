import { Link } from 'react-router-dom';

const Register = () => {
  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow" style={{ width: '100%', maxWidth: '400px' }}>
        <div className="card-body p-4 text-center">
          <h1 className="h5 mb-3">Kayıt kapalı</h1>
          <p className="small text-muted mb-4">
            Bu panele yalnızca atanmış yönetici hesapları ile giriş yapılabilir. Lütfen size verilen e-posta ve
            şifreyi kullanın.
          </p>
          <Link to="/" className="btn btn-primary">
            Giriş sayfasına dön
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Register;
