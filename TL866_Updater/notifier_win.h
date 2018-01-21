#ifndef NOTIFIER_H
#define NOTIFIER_H

#include <QWidget>
#include <QSocketNotifier>


class Notifier : public QWidget
{
    Q_OBJECT
public:
    explicit Notifier(QWidget *parent = 0);

 ~Notifier();
signals:
    void deviceChange(bool arrived);

private:
    QSocketNotifier *socket_notifier;
    void RegisterUsbNotifications();


private:
    bool winEvent(MSG *message, long *result);
#if QT_VERSION >= 0x050000
    bool nativeEvent(const QByteArray& eventType, void* message, long* result);
#endif

};

#endif // NOTIFIER_H
