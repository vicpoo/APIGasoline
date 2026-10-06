// Código compartido por los escenarios de carga (k6).
// Simula a un usuario de la app: inicia sesión de vez en cuando y consulta catálogos.
import http from 'k6/http';
import { check, sleep, group } from 'k6';

export const BASE_URL = __ENV.BASE_URL || 'http://localhost:5234';
const JSON_HEADERS = { headers: { 'Content-Type': 'application/json' } };
const PASSWORD = 'Gasolina2026';

// Se ejecuta UNA vez antes de la prueba: crea un usuario para el login.
export function setup() {
  const email = `carga.${Date.now()}@ejemplo.com`;
  const res = http.post(`${BASE_URL}/api/usuarios`,
    JSON.stringify({ email, password: PASSWORD, estado: 'activo' }), JSON_HEADERS);

  if (res.status !== 201) {
    throw new Error(`No se pudo crear el usuario de prueba (${res.status}). ¿Está encendida la API en ${BASE_URL}?`);
  }
  return { email };
}

// Lo que hace cada usuario virtual en cada iteración.
export function sesionDeUsuario(data) {
  group('Consultas', () => {
    const usuarios = http.get(`${BASE_URL}/api/usuarios`, { tags: { endpoint: 'GET /usuarios' } });
    check(usuarios, { 'usuarios 200': (r) => r.status === 200 });

    const carros = http.get(`${BASE_URL}/api/carros?estado=disponible`, { tags: { endpoint: 'GET /carros' } });
    check(carros, { 'carros 200': (r) => r.status === 200 });

    const empresas = http.get(`${BASE_URL}/api/empresas`, { tags: { endpoint: 'GET /empresas' } });
    check(empresas, { 'empresas 200': (r) => r.status === 200 });
  });

  // 1 de cada 10 iteraciones inicia sesión (el login es lo más pesado: hash PBKDF2)
  if (Math.random() < 0.1) {
    group('Login', () => {
      const login = http.post(`${BASE_URL}/api/auth/login`,
        JSON.stringify({ email: data.email, password: PASSWORD }),
        Object.assign({ tags: { endpoint: 'POST /auth/login' } }, JSON_HEADERS));
      check(login, { 'login 200': (r) => r.status === 200 });

      if (login.status === 200) {
        const perfil = http.get(`${BASE_URL}/api/auth/perfil`, {
          headers: { Authorization: `Bearer ${login.json('token')}` },
          tags: { endpoint: 'GET /auth/perfil' },
        });
        check(perfil, { 'perfil 200': (r) => r.status === 200 });
      }
    });
  }

  // Tiempo que "piensa" un usuario real entre acciones
  sleep(1);
}

// Criterios de aceptación (si no se cumplen, k6 marca la prueba como fallida)
export const umbrales = {
  http_req_failed: ['rate<0.01'],                    // menos de 1 % de errores
  http_req_duration: ['p(95)<500', 'p(99)<1500'],    // 95 % de las peticiones en menos de 500 ms
  checks: ['rate>0.99'],
};
