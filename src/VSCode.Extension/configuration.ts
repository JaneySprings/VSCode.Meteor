import { window, workspace, ExtensionContext } from 'vscode';
import { IProject, IDevice, Target } from "./models";
import { CommandController } from './bridge';
import { UIController } from "./controller";
import * as res from './resources';


export class ConfigurationController {
    public static androidSdkDirectory: string | undefined;
    public static profiler: string | undefined;
    public static project: IProject | undefined;
    public static device: IDevice | undefined;
    public static target: Target | undefined;

    public static activate(context: ExtensionContext) {
        ConfigurationController.androidSdkDirectory = CommandController.androidSdk();
    }

    public static getDebuggingPort(): number {
        if (ConfigurationController.isAndroid()) 
            return ConfigurationController.getSetting(res.configIdMonoSdbDebuggerPortAndroid, res.configDefaultMonoSdbDebuggerPortAndroid);

        if (ConfigurationController.isAppleMobile() && !ConfigurationController.device?.is_emulator) 
            return ConfigurationController.getSetting(res.configIdMonoSdbDebuggerPortApple, res.configDefaultMonoSdbDebuggerPortApple);

        return 0;
    }
    public static getReloadHostPort(): number {
        return ConfigurationController.getSetting<number>(res.configIdHotReloadHostPort, res.configDefaultHotReloadHostPort);
    }
    public static getProfilerPort(): number {
        return ConfigurationController.getSetting<number>(res.configIdProfilerHostPort, res.configDefaultProfilerHostPort);
    }

    public static getUninstallAppOption(): boolean {
        return ConfigurationController.getSetting<boolean>(
            res.configIdUninstallApplicationBeforeInstalling, 
            res.configDefaultUninstallApplicationBeforeInstalling
        );
    }
    public static getTargetFramework(): string | undefined {
        return ConfigurationController.project?.frameworks.find(it => {
            return it.includes(ConfigurationController.device?.platform ?? 'undefined');
        });
    }
    public static getDebuggerOptions(): any {
        return {
            evaluation_timeout: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsEvaluationTimeout),
            member_evaluation_timeout: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsMemberEvaluationTimeout),
            allow_target_invoke: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowTargetInvoke),
            allow_method_evaluation: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowMethodEvaluation),
            allow_to_string_calls: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsAllowToStringCalls),
            flatten_hierarchy: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsFlattenHierarchy),
            group_private_members: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsGroupPrivateMembers),
            group_static_members: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsGroupStaticMembers),
            use_external_type_resolver: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsUseExternalTypeResolver),
            integer_display_format: ConfigurationController.getSettingOrDefault<string>(res.configIdDebuggerOptionsIntegerDisplayFormat),
            current_exception_tag: ConfigurationController.getSettingOrDefault<string>(res.configIdDebuggerOptionsCurrentExceptionTag),
            ellipsize_strings: ConfigurationController.getSettingOrDefault<boolean>(res.configIdDebuggerOptionsEllipsizeStrings),
            ellipsized_length: ConfigurationController.getSettingOrDefault<number>(res.configIdDebuggerOptionsEllipsizedLength)
        };
    }

    public static isMacCatalyst() { return ConfigurationController.device?.platform === 'maccatalyst'; }
    public static isWindows() { return ConfigurationController.device?.platform === 'windows'; }
    public static isAndroid() { return ConfigurationController.device?.platform === 'android'; }
    public static isAppleMobile() { return ConfigurationController.device?.platform === 'ios'; }

    public static isValid(): boolean {
        if (!ConfigurationController.project?.path) {
            window.showErrorMessage(res.messageNoProjectFound);
            return false;
        }
        if (!ConfigurationController.device?.platform) {
            window.showErrorMessage(res.messageNoDeviceFound);
            return false;
        }
        if (!UIController.devices.some(it => it.name === ConfigurationController.device?.name)) {
            window.showErrorMessage(res.messageDeviceNotExists);
            return false;
        }
        if (!ConfigurationController.getTargetFramework()) {
            window.showErrorMessage(res.messageNoFrameworkFound);
            return false;
        }

        return true;
    }
    public static isActive(): boolean {
        return ConfigurationController.project !== undefined && ConfigurationController.device !== undefined;
    }

    private static getSetting<TResult>(id: string, fallback: TResult): TResult {
        return workspace.getConfiguration(res.configId).get(id) ?? fallback;
    }
    private static getSettingOrDefault<TResult>(id: string): TResult | undefined {
        return workspace.getConfiguration(res.configId).get(id);
    }
} 