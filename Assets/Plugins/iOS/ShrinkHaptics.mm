#import <UIKit/UIKit.h>

extern "C" {

void _HapticLight() {
    UIImpactFeedbackGenerator *gen =
        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    [gen prepare];
    [gen impactOccurred];
}

void _HapticMedium() {
    UIImpactFeedbackGenerator *gen =
        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
    [gen prepare];
    [gen impactOccurred];
}

void _HapticHeavy() {
    UIImpactFeedbackGenerator *gen =
        [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
    [gen prepare];
    [gen impactOccurred];
}

void _HapticSuccess() {
    UINotificationFeedbackGenerator *gen = [[UINotificationFeedbackGenerator alloc] init];
    [gen prepare];
    [gen notificationOccurred:UINotificationFeedbackTypeSuccess];
}

void _HapticError() {
    UINotificationFeedbackGenerator *gen = [[UINotificationFeedbackGenerator alloc] init];
    [gen prepare];
    [gen notificationOccurred:UINotificationFeedbackTypeError];
}

} // extern "C"
