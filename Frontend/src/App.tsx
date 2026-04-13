import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import AdminLayout from './components/AdminLayout';
import RequireAuth from './components/RequireAuth';
import Login from './Pages/Login';
import Register from './Pages/Register';
import Dashboard from './Pages/Dashboard';
import CategoryList from './Pages/Category/CategoryList';
import CategoryCreate from './Pages/Category/CategoryCreate';
import CategoryEdit from './Pages/Category/CategoryEdit';
import ProductList from './Pages/Product/ProductList';
import ProductCreate from './Pages/Product/ProductCreate';
import ProductEdit from './Pages/Product/ProductEdit';
import FeatureList from './Pages/Feature/FeatureList';
import FeatureCreate from './Pages/Feature/FeatureCreate';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route element={<RequireAuth />}>
          <Route element={<AdminLayout />}>
            <Route path="dashboard" element={<Dashboard />} />
            <Route path="categories" element={<CategoryList />} />
            <Route path="categories/create" element={<CategoryCreate />} />
            <Route path="categories/edit/:id" element={<CategoryEdit />} />
            <Route path="products" element={<ProductList />} />
            <Route path="products/create" element={<ProductCreate />} />
            <Route path="products/edit/:id" element={<ProductEdit />} />
            <Route path="features" element={<FeatureList />} />
            <Route path="features/create" element={<FeatureCreate />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
