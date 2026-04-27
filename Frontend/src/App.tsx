import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import AdminLayout from './components/AdminLayout';
import RequireAuth from './components/RequireAuth';
import RequireRole from './components/RequireRole';
import Login from './Pages/Login';
import Register from './Pages/Register';
import Dashboard from './Pages/Dashboard';
import KvkkNotice from './Pages/KvkkNotice';
import AdminUsers from './Pages/AdminUsers';
import AdminActivity from './Pages/AdminActivity';
import AdminOrders from './Pages/AdminOrders';
import CategoryList from './Pages/Category/CategoryList';
import CategoryCreate from './Pages/Category/CategoryCreate';
import CategoryEdit from './Pages/Category/CategoryEdit';
import ProductList from './Pages/Product/ProductList';
import ProductCreate from './Pages/Product/ProductCreate';
import ProductEdit from './Pages/Product/ProductEdit';
import ProductPriceUpdate from './Pages/Product/ProductPriceUpdate';
import FeatureList from './Pages/Feature/FeatureList';
import FeatureCreate from './Pages/Feature/FeatureCreate';
import { AuthRoles } from './lib/authRoles';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/kvkk" element={<KvkkNotice />} />
        <Route element={<RequireAuth />}>
          <Route element={<AdminLayout />}>
            <Route path="dashboard" element={<Dashboard />} />
            <Route
              path="admin/users"
              element={
                <RequireRole allow={[AuthRoles.Admin]}>
                  <AdminUsers />
                </RequireRole>
              }
            />
            <Route
              path="admin/activity"
              element={
                <RequireRole allow={[AuthRoles.Admin]}>
                  <AdminActivity />
                </RequireRole>
              }
            />
            <Route
              path="admin/orders"
              element={
                <RequireRole allow={[AuthRoles.Admin]}>
                  <AdminOrders />
                </RequireRole>
              }
            />
            <Route path="categories" element={<CategoryList />} />
            <Route
              path="categories/create"
              element={
                <RequireRole allow={[AuthRoles.Admin, AuthRoles.StaffCategories]}>
                  <CategoryCreate />
                </RequireRole>
              }
            />
            <Route
              path="categories/edit/:id"
              element={
                <RequireRole allow={[AuthRoles.Admin, AuthRoles.StaffCategories]}>
                  <CategoryEdit />
                </RequireRole>
              }
            />
            <Route path="products" element={<ProductList />} />
            <Route
              path="products/create"
              element={
                <RequireRole allow={[AuthRoles.Admin]}>
                  <ProductCreate />
                </RequireRole>
              }
            />
            <Route
              path="products/edit/:id"
              element={
                <RequireRole allow={[AuthRoles.Admin, AuthRoles.StaffPrices, AuthRoles.StaffFeatures]}>
                  <ProductEdit />
                </RequireRole>
              }
            />
            <Route
              path="products/:id/fiyat"
              element={
                <RequireRole allow={[AuthRoles.Admin, AuthRoles.StaffPrices]}>
                  <ProductPriceUpdate />
                </RequireRole>
              }
            />
            <Route path="features" element={<FeatureList />} />
            <Route
              path="features/create"
              element={
                <RequireRole allow={[AuthRoles.Admin, AuthRoles.StaffFeatures]}>
                  <FeatureCreate />
                </RequireRole>
              }
            />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
