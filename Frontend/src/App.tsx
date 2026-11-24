import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import AdminLayout from './components/AdminLayout';
import RequireAuth from './components/RequireAuth';
import RequireRole from './components/RequireRole';
import Login from './Pages/Login';
import { ShopProvider } from './shop/ShopContext';
import ShopLayout from './shop/ShopLayout';
import Home from './shop/pages/Home';
import Catalog from './shop/pages/Catalog';
import ProductPage from './shop/pages/ProductPage';
import CartPage from './shop/pages/CartPage';
import CheckoutPage from './shop/pages/CheckoutPage';
import { OrderDetail, OrderList } from './shop/pages/OrdersPage';
import {
  AccountPage,
  FavoritesPage,
  RequireCustomer,
  ShopLogin,
  ShopRegister,
} from './shop/pages/AccountPages';
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
      <ShopProvider>
      <Routes>
        <Route path="/yonetim" element={<Login />} />
        <Route path="/kvkk" element={<KvkkNotice />} />
        <Route element={<ShopLayout />}>
          <Route index element={<Home />} />
          <Route path="urunler" element={<Catalog />} />
          <Route path="urun/:id" element={<ProductPage />} />
          <Route path="sepet" element={<CartPage />} />
          <Route path="giris" element={<ShopLogin />} />
          <Route path="kayit" element={<ShopRegister />} />
          <Route element={<RequireCustomer />}>
            <Route path="odeme" element={<CheckoutPage />} />
            <Route path="favoriler" element={<FavoritesPage />} />
            <Route path="hesabim" element={<AccountPage />} />
            <Route path="hesabim/siparisler" element={<OrderList />} />
            <Route path="hesabim/siparisler/:id" element={<OrderDetail />} />
          </Route>
        </Route>
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
      </ShopProvider>
    </BrowserRouter>
  );
}

export default App;
