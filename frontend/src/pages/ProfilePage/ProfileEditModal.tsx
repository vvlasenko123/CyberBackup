import React, { useEffect, useRef, useState } from 'react';
import { Icon } from '../../components/Icon';
import {
    ABOUT_MAX_LENGTH,
    AVATAR_TYPES,
    validateAvatar,
    validateProfile,
    type ProfileErrors,
    type ProfileExtras,
    type ProfileUpdate,
} from '../../features/profile/profileApi';

type Props = {
    fullName: string;
    email: string;
    initials: string;
    extras: ProfileExtras;
    onClose: () => void;
    onSave: (update: ProfileUpdate) => Promise<void>;
};

const readAsDataUrl = (file: File) =>
    new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(String(reader.result));
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });

export const ProfileEditModal: React.FC<Props> = ({ fullName, email, initials, extras, onClose, onSave }) => {
    const [form, setForm] = useState<ProfileUpdate>({
        phone: extras.phone,
        telegram: extras.telegram,
        about: extras.about,
        avatarUrl: extras.avatarUrl,
    });
    const [errors, setErrors] = useState<ProfileErrors>({});
    const [saving, setSaving] = useState(false);
    const [submitError, setSubmitError] = useState('');
    const fileRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        const onKey = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && !saving) onClose();
        };
        window.addEventListener('keydown', onKey);
        return () => window.removeEventListener('keydown', onKey);
    }, [onClose, saving]);

    const set = <K extends keyof ProfileUpdate>(key: K, value: ProfileUpdate[K]) => {
        setForm(prev => ({ ...prev, [key]: value }));
        setErrors(prev => ({ ...prev, [key]: undefined }));
    };

    const handleAvatar = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        e.target.value = '';
        if (!file) return;
        const error = validateAvatar(file);
        if (error) {
            setErrors(prev => ({ ...prev, avatar: error }));
            return;
        }
        setErrors(prev => ({ ...prev, avatar: undefined }));
        set('avatarUrl', await readAsDataUrl(file));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const nextErrors = validateProfile(form);
        if (Object.keys(nextErrors).length > 0) {
            setErrors(nextErrors);
            return;
        }
        try {
            setSaving(true);
            setSubmitError('');
            await onSave({
                ...form,
                phone: form.phone.trim(),
                telegram: form.telegram.trim(),
                about: form.about.trim(),
            });
        } catch {
            setSubmitError('Не удалось сохранить профиль. Попробуйте ещё раз.');
            setSaving(false);
        }
    };

    return (
        <div className="profile-modal" role="presentation" onMouseDown={e => e.target === e.currentTarget && !saving && onClose()}>
            <form
                className="profile-modal__dialog"
                role="dialog"
                aria-modal="true"
                aria-labelledby="profile-modal-title"
                onSubmit={handleSubmit}
                noValidate
            >
                <div className="profile-modal__head">
                    <h2 id="profile-modal-title" className="profile-modal__title">Редактирование профиля</h2>
                    <button type="button" className="profile-modal__close" onClick={onClose} aria-label="Закрыть" disabled={saving}>
                        <Icon name="x" size={18} />
                    </button>
                </div>

                <div className="profile-modal__avatar-row">
                    <div className="profile-avatar profile-avatar--md">
                        {form.avatarUrl ? <img src={form.avatarUrl} alt="" /> : initials}
                    </div>
                    <div className="profile-modal__avatar-actions">
                        <div className="profile-modal__avatar-buttons">
                            <button type="button" className="profile-btn profile-btn--ghost" onClick={() => fileRef.current?.click()}>
                                <Icon name="upload" size={15} /> Загрузить фото
                            </button>
                            {form.avatarUrl && (
                                <button type="button" className="profile-btn profile-btn--text" onClick={() => set('avatarUrl', null)}>
                                    Удалить
                                </button>
                            )}
                        </div>
                        <span className={`profile-field__hint${errors.avatar ? ' profile-field__hint--error' : ''}`}>
                            {errors.avatar ?? 'JPG, PNG или WebP, до 5 МБ'}
                        </span>
                        <input
                            ref={fileRef}
                            type="file"
                            accept={AVATAR_TYPES.join(',')}
                            onChange={handleAvatar}
                            hidden
                            data-testid="avatar-input"
                        />
                    </div>
                </div>

                <div className="profile-modal__grid">
                    <label className="profile-field">
                        <span className="profile-field__label">ФИО</span>
                        <input className="profile-field__input" value={fullName} disabled />
                        <span className="profile-field__hint">Меняет администратор</span>
                    </label>

                    <label className="profile-field">
                        <span className="profile-field__label">Email</span>
                        <input className="profile-field__input" value={email} disabled />
                        <span className="profile-field__hint">Используется для входа</span>
                    </label>

                    <label className="profile-field">
                        <span className="profile-field__label">Телефон</span>
                        <input
                            className={`profile-field__input${errors.phone ? ' profile-field__input--error' : ''}`}
                            type="tel"
                            inputMode="tel"
                            placeholder="+7 (900) 123-45-67"
                            value={form.phone}
                            onChange={e => set('phone', e.target.value)}
                            aria-invalid={!!errors.phone}
                        />
                        {errors.phone && <span className="profile-field__hint profile-field__hint--error">{errors.phone}</span>}
                    </label>

                    <label className="profile-field">
                        <span className="profile-field__label">Telegram</span>
                        <input
                            className={`profile-field__input${errors.telegram ? ' profile-field__input--error' : ''}`}
                            placeholder="@username"
                            value={form.telegram}
                            onChange={e => set('telegram', e.target.value)}
                            aria-invalid={!!errors.telegram}
                        />
                        {errors.telegram && <span className="profile-field__hint profile-field__hint--error">{errors.telegram}</span>}
                    </label>

                    <label className="profile-field profile-field--wide">
                        <span className="profile-field__label">О себе</span>
                        <textarea
                            className={`profile-field__input profile-field__textarea${errors.about ? ' profile-field__input--error' : ''}`}
                            rows={4}
                            placeholder="Чем занимаетесь, что интересно в кибербезопасности"
                            value={form.about}
                            onChange={e => set('about', e.target.value)}
                            aria-invalid={!!errors.about}
                        />
                        <span className={`profile-field__hint profile-field__counter${form.about.length > ABOUT_MAX_LENGTH ? ' profile-field__hint--error' : ''}`}>
                            {errors.about ?? ''}
                            <span>{form.about.length} / {ABOUT_MAX_LENGTH}</span>
                        </span>
                    </label>
                </div>

                {submitError && <div className="profile-modal__error">{submitError}</div>}

                <div className="profile-modal__footer">
                    <button type="button" className="profile-btn profile-btn--ghost" onClick={onClose} disabled={saving}>
                        Отмена
                    </button>
                    <button type="submit" className="profile-btn profile-btn--primary" disabled={saving}>
                        {saving ? 'Сохранение...' : 'Сохранить'}
                    </button>
                </div>
            </form>
        </div>
    );
};
