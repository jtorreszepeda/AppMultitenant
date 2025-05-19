# Flujo de Trabajo de Git para AppMultiTenant

Este documento describe el flujo de trabajo de Git que seguiremos en el proyecto AppMultiTenant, basado en el modelo GitFlow.

## Ramas principales

- **`main`** - Contiene el código en producción. Esta rama siempre debe estar estable.
- **`develop`** - Rama de desarrollo principal, contiene las últimas características aprobadas pero que aún no están en producción.

## Ramas de soporte

- **`feature/nombre-caracteristica`** - Ramas para el desarrollo de nuevas características.
- **`bugfix/nombre-bug`** - Ramas para corrección de errores en desarrollo.
- **`hotfix/nombre-hotfix`** - Ramas para correcciones urgentes en producción.
- **`release/vX.Y.Z`** - Ramas para preparar una nueva versión para producción.

## Flujo de trabajo

### Desarrollo de nuevas características

1. Crear una rama de característica desde `develop`:
   ```
   git checkout develop
   git pull
   git checkout -b feature/nombre-caracteristica
   ```

2. Desarrollar la característica con commits frecuentes.

3. Una vez completada la característica, actualizar la rama `develop` y fusionar:
   ```
   git checkout develop
   git pull
   git checkout feature/nombre-caracteristica
   git merge develop  # Resolver conflictos si existen
   git checkout develop
   git merge --no-ff feature/nombre-caracteristica
   git push
   ```

4. Eliminar la rama de característica (opcional):
   ```
   git branch -d feature/nombre-caracteristica
   ```

### Corrección de errores

1. Crear una rama de bugfix desde `develop`:
   ```
   git checkout develop
   git pull
   git checkout -b bugfix/nombre-bug
   ```

2. Corregir el error.

3. Fusionar con `develop` de manera similar a las características.

### Correcciones urgentes en producción (hotfixes)

1. Crear una rama de hotfix desde `main`:
   ```
   git checkout main
   git pull
   git checkout -b hotfix/nombre-hotfix
   ```

2. Corregir el error urgente.

3. Fusionar con `main` Y con `develop`:
   ```
   git checkout main
   git merge --no-ff hotfix/nombre-hotfix
   git push
   git checkout develop
   git merge --no-ff hotfix/nombre-hotfix
   git push
   git branch -d hotfix/nombre-hotfix
   ```

### Preparación de versiones

1. Crear una rama de release desde `develop`:
   ```
   git checkout develop
   git pull
   git checkout -b release/vX.Y.Z
   ```

2. Realizar ajustes finales, correcciones menores y actualizar versión.

3. Cuando la versión está lista, fusionar con `main` y `develop`:
   ```
   git checkout main
   git merge --no-ff release/vX.Y.Z
   git tag -a vX.Y.Z -m "Versión X.Y.Z"
   git push --follow-tags
   git checkout develop
   git merge --no-ff release/vX.Y.Z
   git push
   git branch -d release/vX.Y.Z
   ```

## Convenciones de commit

Usaremos Commits Convencionales para mantener un historial claro:

- `feat:` - Nueva característica
- `fix:` - Corrección de error
- `docs:` - Cambios en documentación
- `style:` - Cambios de formato (que no afectan el código)
- `refactor:` - Refactorización de código
- `test:` - Añadir o corregir pruebas
- `chore:` - Tareas de mantenimiento

Ejemplo: `feat: implementar autenticación de usuarios por tenant` 