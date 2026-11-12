// Código compartido por los escenarios de carga (k6).
// Simula a un usuario de la app: inicia sesión y consulta los listados más usados.
//
// Las credenciales NO van en el código: se pasan por variables de entorno (ver LEEME.md).
//   k6 run -e K6_EMAIL=admin.a@demo.local -e K6_PASSWORD=... pruebas-carga/1-humo.js
import http from 'k6/http';
import { check, sleep, group, fail } from 'k6';

export const BASE_URL = __ENV.BASE_URL || 'http://localhost:5234';
const EMAIL = __ENV.K6_EMAIL;
const PASSWORD = __ENV.K6_PASSWORD;
const PROBAR_LOGIN = __ENV.PROBAR_LOGIN === '1';
const JSON_HEADERS = { 'Content-Type': 'application/json' };

// Token de cada usuario virtual (cada VU tiene su propia copia de este módulo)
let token = null;

function iniciarSesion(tags) {
  const res = http.post(`${BASE_URL}/api/auth/login`,
    JSON.stringify({ email: EMAIL, password: PASSWORD }),
    { headers: JSON_HEADERS, tags: tags || { endpoint: 'POST /auth/login' } });
  return res.status === 200 ? res.json('token') : null;
}

// Se ejecuta UNA vez antes de la prueba: revisa que las credenciales sirvan.
export function setup() {
  if (!EMAIL || !PASSWORD) {
    fail('Faltan las credenciales: k6 run -e K6_EMAIL=... -e K6_PASSWORD=... (ver pruebas-carga/LEEME.md).');
  }

  if (!iniciarSesion()) {
    fail(`No se pudo iniciar sesión como ${EMAIL} en ${BASE_URL}. ¿Está encendida la API y el usuario existe?`);
  }

  return {};
}

function get(url, endpoint) {
  if (!token) token = iniciarSesion();

  const params = () => ({ headers: { Authorization: `Bearer ${token}` }, tags: { endpoint } });
  let res = http.get(url, params());

  // El token de acceso dura poco (Jwt:ExpiracionMinutos): si venció, vuelve a entrar y reintenta
  if (res.status === 401) {
    token = iniciarSesion();
    res = http.get(url, params());
  }

  return res;
}

// Lo que hace cada usuario virtual en cada iteración.
export function sesionDeUsuario() {
  group('Consultas', () => {
    const carros = get(`${BASE_URL}/api/carros?estado=disponible`, 'GET /carros');
    check(carros, { 'carros 200': (r) => r.status === 200 });

    const viajes = get(`${BASE_URL}/api/viajes?tamanoPagina=20`, 'GET /viajes');
    check(viajes, { 'viajes 200': (r) => r.status === 200 });

    const notificaciones = get(`${BASE_URL}/api/notificaciones?soloNoLeidas=true`, 'GET /notificaciones');
    check(notificaciones, { 'notificaciones 200': (r) => r.status === 200 });

    const perfil = get(`${BASE_URL}/api/auth/perfil`, 'GET /auth/perfil');
    check(perfil, { 'perfil 200': (r) => r.status === 200 });
  });

  // Opcional (PROBAR_LOGIN=1): 1 de cada 10 iteraciones mide el login, lo más pesado (hash PBKDF2).
  // Requiere la API con LimiteAuth__Habilitado=false: si no, el límite por IP responde 429.
  if (PROBAR_LOGIN && Math.random() < 0.1) {
    group('Login', () => {
      check(iniciarSesion({ endpoint: 'POST /auth/login' }), { 'login 200': (t) => t !== null });
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
